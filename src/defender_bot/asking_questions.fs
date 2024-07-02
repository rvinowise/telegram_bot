namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
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
    
    let ask_question
        (bot: ITelegramBotClient)
        target
        (question: Question)
        =
        let buttons =
            question.answers
            |>shuffle_sequence
            |>Seq.map(fun answer ->
                InlineKeyboardButton.WithCallbackData(answer.text, answer.text);
            )
            |>Array.ofSeq
        
        let (User_id target_value) = target
            
        bot.SendTextMessageAsync(
            target_value,
            (question.text),
            replyMarkup=InlineKeyboardMarkup(buttons)
        )
    
    let ask_questions_of_stranger
        (bot: ITelegramBotClient)
        database
        (stranger: User)
        =
        let stranger_chat = (ChatId stranger.Id)
        
        task {
            bot.SendTextMessageAsync(
                stranger_chat,
                sprintf "Answer questions to join %s" "the group"
            )|>ignore
            
            //ask_question bot stranger_chat question1
            //ask_question bot stranger_chat question2
            
            
            
        }:> Task
        
    
    let questioning_result
        database
        (user)
        group
        =
        let (questions_amount, score) =
            User_database.read_account_score_in_group
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
        (user)
        group
        =
        let answered_questions =
            User_database.read_answered_questions
                database user group
            |>Set.ofSeq
                
        let all_group_questions =
            User_database.read_group_questions database group
            |>Set.ofSeq
            
        let novel_questions = 
            answered_questions
            |>Set.difference all_group_questions
        
        novel_questions
        |>Seq.item (Random().Next(novel_questions.Count))
        |>Question_database.read_question
            database
            group
                
      
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
            new_question_for_user database user group
            |>ask_question bot user :> Task