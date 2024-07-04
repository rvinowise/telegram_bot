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




module Handling_updates =
    
    
    
    
    let on_button_clicked
        (bot: ITelegramBotClient)
        database
        (update: Update)
        (cancellationToken: CancellationToken)
        =
        match
            JsonValue.TryParse(update.CallbackQuery.Data)
        with
        |Some click_data ->
            match
                click_data.TryGetProperty("action")
                |>(Option.map (_.AsString()))
            with
            |None ->
                $"action of the click data shouldn't be empty: {click_data}"
                |>Log.error|>ignore
                Task.CompletedTask
            |Some Callback_language.user_asked_to_join ->
                Welcoming_strangers.on_user_asked_to_join
                    bot
                    database
                    (User_id update.CallbackQuery.From.Id)
                    (Group_id.from_string_chat update.CallbackQuery.ChatInstance)
                    update.CallbackQuery.Id
            |Some Callback_language.user_answered_question_about_group ->
                Asking_questions.on_user_answered_quesiton
                    bot
                    database
                    (User_id update.CallbackQuery.From.Id)
                    (Group_id.from_string_chat update.CallbackQuery.ChatInstance)
                    update.CallbackQuery.Id
            | Some unknown_action ->
                $"unknown callback action {unknown_action}"
                |>Log.error|>ignore
                Task.CompletedTask
        |None ->
            $"can't parse the json data of a click: {update.CallbackQuery.Data}"
            |>Log.error|>ignore
            Task.CompletedTask
        
  
            
            
    let is_command (text:string) =
        text
        |>Seq.tryHead
        |>function
        |Some symbol -> symbol = '/'
        |None -> false    
    
    let handle_update
        (bot: ITelegramBotClient)
        database
        (update: Update)
        (cancellationToken: CancellationToken)
        =
        if (update.CallbackQuery|>isNull|>not) then
            on_button_clicked
                bot
                database
                update
                cancellationToken
            
        elif (isNull update.Message) then
            Task.CompletedTask
        else
            let chatId = ChatId update.Message.Chat.Id
            let group = Group_id.try_from_chat chatId //still can be a private chat with a user!
            
            match group with
            |Some group ->
                if update.Message.NewChatMembers|>isNull|>not then
                    Welcoming_strangers.handle_joined_users
                        bot
                        database
                        (group)
                        update.Message.NewChatMembers
                    :>Task
                    
                else 
                    Task.CompletedTask
            |None -> Task.CompletedTask
            
            

type Update_handler()  =
    
    member this.database = Database.open_connection()
    member this.untested_strangers: List<User> = []
    
    interface IUpdateHandler with
        member this.HandleUpdateAsync(
            bot: ITelegramBotClient ,
            update: Update ,
            cancellationToken: CancellationToken )
            =
            Handling_updates.handle_update
                bot
                this.database
                update
                cancellationToken
            

              
        member this.HandlePollingErrorAsync(
            bot: ITelegramBotClient,
            exc: Exception,
            cancellationToken: CancellationToken 
            )
            =
            $"{exc.Message}"
            |>Log.error|>ignore
            Task.CompletedTask



