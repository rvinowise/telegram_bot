namespace rvinowise.telegram_defender

open System
open System.Collections
open System.Security
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups
open rvinowise.telegram_defender.database_schema


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
            
    
    let precise_user_destription (user:User) =
        let is_premium =
            if (user.IsPremium.GetValueOrDefault(false) = true) then
                "premium"
            else ""
        $"{user.FirstName} {user.LastName} {user.Username} id={user.Id} {is_premium}"
    
    let user_destription (user:User) =
        [
            Some user.FirstName;
            match user.LastName with
            | ""|null -> None
            | surname -> Some surname;
            match user.Username with
            | null -> None
            | username -> Some $"(@%s{username})"
        ]
        |>List.choose id
        |>String.concat " "
            
    
    
    let handle_joined_strangers
        (bot: ITelegramBotClient)
        database
        group
        (strangers: User array)
        =
        let language =
            Group_policy_database.read_language
                database group
        
        let buttons =
            [|
                InlineKeyboardButton.WithUrl(
                    (Localised_text.text language Localised_text.contact_defender_bot),
                    $"{Settings.bot_url}?start={Callback_language.user_asked_to_join}"
                );
            |]
        
        let strangers_names =
            strangers
            |>Array.map user_destription
            |>String.concat ",\n"

        task {
            strangers
            |>Array.map (fun user ->
                user.Id
                |>User_id
                |>Unauthorised_strangers_database.write_joined_stranger
                    database
                    group
                
                user.Id
                |>User_id
                |>Executing_jugements.restrict_joined_stranger bot group
            )
            |>ignore
            
            let welcoming_message =
                (Localised_text.text
                    language
                    Localised_text.welcoming_newcomers
                ,
                [|strangers_names :> obj|])
                ||>Localised_text.format
            
            let! welcoming =
                bot.SendTextMessageAsync(
                    Group_id.asChatId group,
                    welcoming_message,
                    replyMarkup=InlineKeyboardMarkup(buttons)
                )
            
            Task.Run(fun () ->
                Task.Delay 30000 |>Task.WaitAll
                bot.DeleteMessageAsync(Group_id.asChatId group, welcoming.MessageId)
            )|>ignore
            
            return welcoming
        }
    
    let handle_joined_users
        (bot: ITelegramBotClient)
        database
        group
        (update: Update)
        =
        let strangers =
            update.Message.NewChatMembers
            |>Array.filter (fun user ->
                Asking_questions.questioning_final_conclusion_from_database
                    database
                    (User_id user.Id)
                    group
                    =
                    Stranger
            )
        if (strangers.Length > 0) then
            Group_gist_database.write_title
                database
                group
                update.Message.Chat.Title
                
            handle_joined_strangers bot database group strangers :> Task
        else
            Task.CompletedTask
    
    
    
    
    let dismiss_click
        (bot: ITelegramBotClient)
        group_language
        click
        =
        bot.AnswerCallbackQueryAsync(
            click,
            (Localised_text.text group_language Localised_text.dismissing_click )
        )
        
            
    // let parse_user_asking_to_join
    //     (bot: ITelegramBotClient)
    //     database
    //     user
    //     (callback_data: JsonValue)
    //     (callback_query: CallbackQuery)
    //     =
    //     try
    //         let joining_group =
    //             callback_data.GetProperty(Callback_language.group).AsInteger64()
    //             |>Group_id
    //       
    //         Group_gist_database.write_title
    //             database
    //             joining_group
    //             callback_query.Message.Chat.Title
    //             
    //         respond_to_user_clicking_joining_button
    //             bot
    //             database
    //             user
    //             joining_group
    //             callback_query.Id
    //     with
    //     | :? JsonException as exc ->
    //         $"json callback {callback_data} of asking to join a group is bad: {exc.Message}"
    //         |>Log.error|>Bot_exception|>raise