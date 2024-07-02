namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
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
            if (user.IsPremium = true) then
                "premium"
            else ""
        $"{user.FirstName} {user.LastName} {user.Username} id={user.Id} {is_premium}"
    
    
    let user_asked_to_join = "user_asked_to_join"
    
    let handle_joined_strangers
        (bot: ITelegramBotClient)
        chat
        (strangers: User array)
        =
        let buttons =
            [|
                InlineKeyboardButton.WithCallbackData("Let me in", user_asked_to_join);
            |]
        
        let strangers_names =
            strangers
            |>Array.map user_destription
            |>String.concat "\n"

        task {
            strangers
            |>Array.map (fun user -> 
                Executing_jugements.restrict_joined_stranger bot chat user.Id
            )
            |>ignore
            
            let! welcoming =
                bot.SendTextMessageAsync(
                    chat,
                    ($"Hi, {strangers_names}\nTo join, prove that you're genuine by clicking a button below:"),
                    replyMarkup=InlineKeyboardMarkup(buttons)
                )
            
            remember_welcoming bot chat welcoming.MessageId strangers
            
            Task.Run(fun () ->
                Task.Delay 30000 |>Task.WaitAll
                bot.DeleteMessageAsync(chat, welcoming.MessageId)
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
    
    
    
    let is_stranger
        database
        user
        =
        User_database.is_genuine_member database user
        |>not
    
    let dismiss_click
        (bot: ITelegramBotClient)
        click
        =
        bot.AnswerCallbackQueryAsync(click,"this button is for another user")
        
    let on_user_asked_to_join
        (bot: ITelegramBotClient)
        database
        (user: User)
        click
        =
        if (is_stranger database user.Id) then
            Asking_questions.ask_questions_of_stranger bot database user
        else
            dismiss_click bot click