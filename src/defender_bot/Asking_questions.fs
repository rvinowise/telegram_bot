namespace rvinowise.telegram_defender

open System
open System.Collections
open System.Security
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups


type Questioning_result =
    |Passed
    |Failed
    |Indecisive
    |Stranger


module Asking_questions =
    let shuffle_sequence (items) =
        let rnd = System.Random ()
        items |> Seq.sortBy(fun _ -> rnd.Next(1, 52) )
    
    let json_callback_data_for_answer_button
        question
        (answer: Answer)
        =
        JsonValue.Record [|
            Callback_language.action,
            JsonValue.String Callback_language.user_answered_question_about_group;
            
            Callback_language.answering_question,
            JsonValue.Record ([| 
                Callback_language.question_id,
                (question |>Question.primary_key |>Question_id.value |>JsonValue.String)
                
                Callback_language.about_group,
                (question.group |> Group_id.value |>decimal|>JsonValue.Number );
                
                Callback_language.given_answer,
                (JsonValue.String answer.text);
                
            |])
        |]
    
    let write_callback_data_for_answer_button
        database
        question
        (answer: Answer)
        =
        let button = Button_id.create()
        json_callback_data_for_answer_button question answer
        |>Interaction_interface_database.write_button_callback_data
            database
            button
        button    
            
    
    let ask_question
        (bot: ITelegramBotClient)
        database
        target
        (question: Question)
        =
        User_questioning_database.write_asked_question
            database
            target
            question
        
        let buttons =
            question.answers
            |>shuffle_sequence
            |>Seq.map(fun answer ->
                {
                    Message_button.text = answer.text
                    callback_data =
                        write_callback_data_for_answer_button
                            database
                            question
                            answer
                        |>Button_id.value
                }
            )|>Seq.map (Seq.singleton)
        
        Telegram.send_message_with_buttons
            bot
            (User_id.to_Chat_id target)
            question.text
            buttons
        
    
        
    let questioning_final_conclusion
        database
        group
        questions_amount
        score
        =
        let questining_harshness =
            Group_policy_database.read_questioning_harshness
                database group
                
        if questions_amount < questining_harshness.maximum_questions_amount then
            Indecisive
        elif score < questining_harshness.minimum_accepted_score then
            Failed
        else
            Passed
    let questioning_final_conclusion_from_database
        database
        (user)
        group
        =
        match
            User_questioning_database.read_tried_questions
                database user group
            |>List.ofSeq
        with
        | [] -> Stranger
        | asked_questions ->
            User_questioning_database.read_account_score_in_group
                database
                user
                group
            ||>questioning_final_conclusion database group
    
    let new_question_for_user
        database
        about_group
        (user)
        =
        let answered_questions =
            User_questioning_database.read_tried_questions
                database user about_group
            |>Set.ofSeq
                
        let all_group_questions =
            User_questioning_database.read_group_questions database about_group
            |>Set.ofSeq
            
        let novel_questions = 
            answered_questions
            |>Set.difference all_group_questions
        
        if (novel_questions.Count > 0) then
            novel_questions
            |>Seq.item (Random().Next(novel_questions.Count))
            |>Question_database.read_question
                database
                about_group
            |>Some
        else None
            
    
    let ask_new_question_of_user
        (bot: ITelegramBotClient)
        database
        group
        user
        =
        new_question_for_user database group user 
        |>function
        |Some question ->
            ask_question bot database user question :> Task
        |None ->
            $"no novel questions about group {Group_id.value group} for user {User_id.value user}"
            |>Log.error|>ignore
            Task.CompletedTask
    
    let start_questioning_stranger_about_group
        (bot: ITelegramBotClient)
        database
        about_group
        (stranger: User_id)
        =
        //let stranger_chat = (User_id.to_ChatId stranger)
        
        let group_language =
            Group_policy_database.read_language
                database
                about_group
        
        let group_title =
            Group_gist_database.read_title
                database about_group
        
        let rules_message =
            Localised_text.format
                (Localised_text.text group_language Localised_text.answer_questions_prompt)
                [|group_title :> obj|]
        
        task {
            Telegram.send_message
                bot
                (User_id.to_Chat_id stranger)
                rules_message
            |>ignore
            
            return
                ask_new_question_of_user
                    bot
                    database
                    about_group
                    stranger
                
        }:> Task
    
    let start_questioning_about_next_group
        (bot: ITelegramBotClient)
        database
        (user: User_id)
        =
        match 
            Unauthorised_strangers_database.read_groups_in_which_user_is_stranger
                database
                user
            |>List.ofSeq
        with
        |[] -> Task.CompletedTask
        |next_indecisive_group::_ ->
            start_questioning_stranger_about_group
                bot
                database
                next_indecisive_group
                user
     
    let evaluate_answer
        (bot: ITelegramBotClient)
        database
        (user)
        answer_text
        question
        =
        let about_group = question.group
        let score_change = Question.answer_score question answer_text
        
        User_questioning_database.write_given_answer_to_question
            database
            user
            question
            answer_text
        
        let questioning_result =
            User_questioning_database.change_account_questioning_score
                database
                user
                about_group
                score_change
            ||>questioning_final_conclusion database about_group
        
        
        match questioning_result with
            
        |Passed ->
            task{
                Executing_jugements.make_friend bot database about_group user |>Task.WaitAll
                start_questioning_about_next_group bot database user |>Task.WaitAll
            } :> Task
        |Failed ->
            task{
                //Executing_jugements.make_foe bot database about_group user |>Task.WaitAll
                Executing_jugements.announce_failure_in_test bot database about_group user |>Task.WaitAll
                start_questioning_about_next_group bot database user |>Task.WaitAll
            } :> Task
                    
        |Indecisive|Stranger ->
            ask_new_question_of_user bot database about_group user
            
            
    let parse_answered_question
        (bot: ITelegramBotClient)
        database
        (user)
        (click: JsonValue)
        =
        
        try
            let answering_question = click.GetProperty(Callback_language.answering_question)
            let question =
                answering_question.GetProperty(Callback_language.question_id).AsString()
            let about_group =
                answering_question.GetProperty(Callback_language.about_group).AsInteger64()
            let given_answer =
                answering_question.GetProperty(Callback_language.given_answer).AsString()
            
            Question_database.read_question
                database
                (Group_id about_group)
                (Question_id question)
            |>evaluate_answer
                bot
                database
                user
                given_answer
        with
        | :? JsonException as exc ->
            $"json callback {click} of answering a question is bad: {exc.Message}"
            |>Log.error|>Bot_exception|>raise
            
    let handle_click_answering_question
        (bot: ITelegramBotClient)
        database
        (user)
        (click_data: JsonValue)
        (callback_query:CallbackQuery)
        =
        task {
            if isNull callback_query.Message then
                Task.CompletedTask
            else
                Telegram.delete_message
                    bot
                    (User_id.to_Chat_id user)
                    callback_query.Message.MessageId
               
            |>Task.WaitAll
            
            parse_answered_question
                bot
                database
                (User_id callback_query.From.Id)
                click_data
            |>Task.WaitAll
        } :> Task