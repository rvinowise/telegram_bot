namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
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
        if
            (update.CallbackQuery.Data = Welcoming_strangers.user_asked_to_join)
        then
            Welcoming_strangers.on_user_asked_to_join
                bot
                database
                update.CallbackQuery.From
                update.CallbackQuery.Id
        else
            Task.CompletedTask
            
            //(update.CallbackQuery.Message. = Asking_questions.user_answered_question)
            
            
    
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
            
            if update.Message.NewChatMembers|>isNull|>not then
                Welcoming_strangers.handle_joined_users
                    bot
                    database
                    chatId
                    update.Message.NewChatMembers
                
            else 
                Task.CompletedTask


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

            Task.CompletedTask



