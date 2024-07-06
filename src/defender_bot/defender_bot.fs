namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Telegram.Bot
open Telegram.Bot.Exceptions
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.Enums
open Telegram.Bot.Types.ReplyMarkups




module Handling_updates =
    
    
    
    
    let on_button_clicked
        (bot: ITelegramBotClient)
        database
        (update: Update)
        (cancellationToken: CancellationToken)
        =
        try
            match
                update.CallbackQuery.Data
                |>Button_id
                |>Interaction_interface_database.read_button_callback_data
                    database
            with
            |Some click_data ->
                match
                    click_data.TryGetProperty("action")
                    |>(Option.map (_.AsString()))
                with
                |Some Callback_language.user_answered_question_about_group ->
                    Asking_questions.handle_click_answering_question
                        bot
                        database
                        (User_id update.CallbackQuery.From.Id)
                        click_data
                        update.CallbackQuery
                        
                | Some unknown_action ->
                    $"unknown callback action {unknown_action}"
                    |>Log.error|>ignore
                    Task.CompletedTask
                |None ->
                    $"action of the click data shouldn't be empty: {click_data}"
                    |>Log.error|>ignore
                    Task.CompletedTask
            |None ->
                $"the button_id {update.CallbackQuery.Data} doesn't exist in the database of button callbacks"
                |>Log.error|>ignore
                Task.CompletedTask
        with
        | exc -> //ApiRequestException
            $"responding to a button clicked raised an exception {exc.GetType()}: {exc.Message}"
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
        try
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
                
                if (
                    (update.Message.Text |>isNull|>not)
                    &&
                    (update.Message.Text.StartsWith("/start"))
                    )
                then
                    let user = User_id (chatId.Identifier.GetValueOrDefault(0))
                    Asking_questions.start_questioning_about_next_group
                        bot
                        database
                        user
                else
                    match group with
                    |Some group ->
                        if update.Message.NewChatMembers|>isNull|>not then
                            Welcoming_strangers.handle_joined_users
                                bot
                                database
                                group
                                update
                            
                        else 
                            Task.CompletedTask
                    |None -> Task.CompletedTask
        with
        | exc -> //ApiRequestException
            $"responding to a button clicked raised an exception {exc.GetType()}: {exc.Message}"
            |>Log.error|>ignore
            Task.CompletedTask    
            



type Update_handler()  =
    
    member this.database = Database.open_connection()
    
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
            $"the update handler caught an exception (the bot is down now?): {exc.Message}"
            |>Log.error|>ignore
            raise exc



module Telegram_service =
    let start () =
        let bot = TelegramBotClient(Settings.bot_token)
        
        //Preparing_commands.prepare_commands bot |>Async.AwaitTask|>ignore
        
        let receiverOptions =
            ReceiverOptions(
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            )
        
        bot.StartReceiving(
            Update_handler(),
            receiverOptions
        )
        
        bot.GetMeAsync()|>Async.AwaitTask