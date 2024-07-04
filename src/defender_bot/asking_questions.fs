namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups


type Questioning_result =
    |Friend
    |Foe
    |Indecisive


module Asking_questions =
    let shuffle_sequence (items) =
        let rnd = System.Random ()
        items |> Seq.sortBy(fun _ -> rnd.Next(1, 52) )
    
    let callback_data_for_answer_button_JSON
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
        |]|>string
    
    let callback_data_for_answer_button
        question
        (answer: Answer)
        =
        Guid.NewGuid().ToString()
    
    let ask_question
        (bot: ITelegramBotClient)
        target
        (question: Question)
        =
        let buttons =
            question.answers
            |>shuffle_sequence
            |>Seq.map(fun answer ->
                InlineKeyboardButton.WithCallbackData(
                    answer.text,
                    (callback_data_for_answer_button question answer)
                );
            )
            |>Array.ofSeq
        
        let (User_id target_value) = target
            
        bot.SendTextMessageAsync(
            target_value,
            (question.text),
            replyMarkup=InlineKeyboardMarkup(buttons)
        )
    
    
        
    
    let questioning_result
        database
        (user)
        group
        =
        let (questions_amount, score) =
            User_questioning_database.read_account_score_in_group
                database
                user
                group
        if questions_amount < Settings.maximum_questions_amount then
            Indecisive
        elif score < Settings.minimum_accepted_score then
            Foe
        else
            Friend
    
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
        
        novel_questions
        |>Seq.item (Random().Next(novel_questions.Count))
        |>Question_database.read_question
            database
            about_group
    
    let ask_new_question_of_user
        (bot: ITelegramBotClient)
        database
        group
        user
        =
        new_question_for_user database group user 
        |>ask_question bot user :> Task
    
    let ask_questions_of_stranger
        (bot: ITelegramBotClient)
        database
        about_group
        (stranger: User_id)
        =
        let stranger_chat = (User_id.asChatId stranger)
        
        task {
            bot.SendTextMessageAsync(
                stranger_chat,
                sprintf "Answer questions to join %s" "the group"
            )|>ignore
            
            return
                ask_new_question_of_user
                    bot
                    database
                    about_group
                    stranger
                
        }:> Task
     
    let on_user_answered_quesiton
        (bot: ITelegramBotClient)
        database
        (user)
        group
        click
        =
        match (questioning_result database user group) with
        |Friend ->
            Executing_jugements.make_friend bot group user 
            
        |Foe ->
            Executing_jugements.make_foe bot group user 
        |Indecisive ->
            ask_new_question_of_user bot database group user