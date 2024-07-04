namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups





module Welcoming_strangers =
    
    let delete_welcoming
        (bot: ITelegramBotClient)
        chat
        welcoming
        =
        bot.DeleteMessageAsync(chat, welcoming)
    
    
    
    let remember_welcoming
        (bot: ITelegramBotClient)
        chat
        welcoming
        for_users
        =
        ()
            
    
    let user_destription (user:User) =
        let is_premium =
            if (user.IsPremium.GetValueOrDefault(false) = true) then
                "premium"
            else ""
        $"{user.FirstName} {user.LastName} {user.Username} id={user.Id} {is_premium}"
    
    let callback_data_for_joining_button()
        =
        JsonValue.Record [|
            Callback_language.action,
            JsonValue.String Callback_language.user_asked_to_join;
        |]|>string
    
    let handle_joined_strangers
        (bot: ITelegramBotClient)
        group
        (strangers: User array)
        =
        let buttons =
            [|
                InlineKeyboardButton.WithCallbackData(
                    "Let me in",
                    callback_data_for_joining_button());
            |]
        
        let strangers_names =
            strangers
            |>Array.map user_destription
            |>String.concat "\n"

        task {
            strangers
            |>Array.map (fun user ->
                user.Id
                |>User_id
                |>Executing_jugements.restrict_joined_stranger bot group
            )
            |>ignore
            
            let! welcoming =
                bot.SendTextMessageAsync(
                    Group_id.asChatId group,
                    ($"Hi, {strangers_names}\nTo join, prove that you're genuine by clicking a button below:"),
                    replyMarkup=InlineKeyboardMarkup(buttons)
                )
            
            remember_welcoming bot group welcoming.MessageId strangers
            
            Task.Run(fun () ->
                Task.Delay 30000 |>Task.WaitAll
                bot.DeleteMessageAsync(Group_id.asChatId group, welcoming.MessageId)
            )
            
            return welcoming
        }
    
    let handle_joined_users
        (bot: ITelegramBotClient)
        database
        group
        (users: User array)
        =
        let strangers =
            users
            |>Array.filter (fun user ->
                Asking_questions.questioning_result
                    database
                    (User_id user.Id)
                    group
                    =
                    Indecisive
            )
    
        handle_joined_strangers bot group strangers
    
    
    
    
    let dismiss_click
        (bot: ITelegramBotClient)
        click
        =
        bot.AnswerCallbackQueryAsync(click,"this button is for another user")
        
    let on_user_asked_to_join
        (bot: ITelegramBotClient)
        database
        (user)
        group
        click
        =
        match
            Asking_questions.questioning_result
                database
                user
                group
        with
        |Indecisive ->
            Asking_questions.ask_questions_of_stranger bot database group user
        |_ ->
            dismiss_click bot click