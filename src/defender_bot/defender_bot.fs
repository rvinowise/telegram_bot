namespace rvinowise.telegram_defender

open System
open System.Diagnostics
open System.IO
open System.Net.Mime
open System.Security
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Microsoft.VisualStudio.TestPlatform.Common.Utilities
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
        $"""start on_button_clicked"""|>Log.debug
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
            
    
    let on_contacting_bot bot database (update:Update) =
        let chatId = ChatId update.Message.Chat.Id
        let user = User_id (chatId.Identifier.GetValueOrDefault(0))
        Asking_questions.start_questioning_about_next_group
            bot
            database
            user
    
    
    let remember_administrators_as_ignored
        (bot: ITelegramBotClient)
        database
        group
        =
        task {
            let! admins =
                Telegram.get_admins
                    bot group
            admins
            |>Array.iter (fun admin ->
                admin.User.Id
                |>User_id
                |>Ignored_members_database.write_ignored_member
                    database
                    group
            )
        }
    
    let on_bot_added_to_group
        (bot: ITelegramBotClient)
        database
        (update: Update)
        =
        let group = Telegram_group.try_group_from_update update
        $"bot is added to group {group}"|>Log.info
        
        match group with
        |Some group ->
            remember_administrators_as_ignored
                bot
                database
                group
        |None ->
            $"bot is added to group, but group is null in the update"
            |>Bot_exception|>raise
        
    let on_new_members
        bot
        database
        (update: Update)
        =
        let chatId = ChatId update.Message.Chat.Id
        let group = Group_id.try_from_chat chatId //still can be a private chat with a user!
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
        |None ->
            $"""there are new members, but no group is specified in the unpdate"""
            |>Bot_exception|>raise
    
    
        
           
    let is_command (text:string) =
        text
        |>Seq.tryHead
        |>function
        |Some symbol -> symbol = '/'
        |None -> false    
    
    let is_button_clicked (update:Update) =
        update.CallbackQuery|>isNull|>not
        
    let is_contacting_bot (update:Update) =
        (update.Message |> isNull |> not)
        &&
        (update.Message.Text |>isNull|>not)
        &&
        update.Message.Text.StartsWith("/start")
    
    let is_chat_message (update:Update) =
        (update.Message |> isNull |> not)
        &&
        (isNull update.Message.NewChatMembers)
    
    let is_bot_the_only_new_member
        (bot: ITelegramBotClient)
        (update: Update)
        =
        (update.Message.NewChatMembers
        |>Array.length = 1)
        &&
        (update.Message.NewChatMembers
        |>Array.head
        |>_.Id
        |>(=)bot.BotId)
        
    let is_bot_among_new_members
        (bot: ITelegramBotClient)
        (update: Update)
        =
        (update.Message.NewChatMembers|>isNull|>not)
        &&
        update.Message.NewChatMembers
        |>Array.map (_.Id)
        |>Array.exists ((=)bot.BotId)
    
    let is_new_members
        (bot: ITelegramBotClient)
        (update: Update)
        =
        (update.Message |> isNull |> not)
        &&
        (update.Message.NewChatMembers|>isNull|>not)
        &&
        (is_bot_the_only_new_member bot update|>not)
    
    let try_new_members  (update: Update) =
        let new_members =
            if
                (update.Message |> isNull |> not)
                &&
                (update.Message.NewChatMembers|>isNull|>not)
            then
                update.Message.NewChatMembers|>List.ofArray
            else []
        let chatId = ChatId update.Message.Chat.Id
        let group = Group_id.try_from_chat chatId //still can be a private chat with a user!
        
        match group,new_members with
        |None, [] -> None
        |None, new_member::tail ->
            $"""there are new members: {Log.to_json new_member}
            but no group is specified in the unpdate"""
            |>Bot_exception|>raise
        |Some group, new_members -> Some (group, new_members)
    
    
    let is_bot_added_to_group
        (bot: ITelegramBotClient)
        (update: Update)
        =
        (update.Message |> isNull |> not)
        &&
        (update.Message.NewChatMembers|>isNull|>not)
        &&
        (is_bot_among_new_members bot update)
    

    let check_if_bot_is_admin
        (bot: ITelegramBotClient)
        =
        ()
        
        
    let handle_update
        (bot: ITelegramBotClient)
        database
        (update: Update)
        (cancellationToken: CancellationToken)
        =
        $"""start handle_update:
        {Log.to_json update}
        """
        |>Log.debug
        
        if
            is_button_clicked update
        then
            on_button_clicked
                bot
                database
                update
                cancellationToken
        elif
            is_contacting_bot update
        then
            on_contacting_bot
                bot
                database
                update
        elif
            is_chat_message update
        then
            Catching_strangers.check_new_messages
                bot
                database
                update
        elif
            is_new_members bot update
        then
            //Task.CompletedTask
            on_new_members
                bot
                database
                update
        elif
            is_bot_added_to_group bot update
        then
            on_bot_added_to_group
                bot
                database
                update    
        else
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
            

              
        member this.HandleErrorAsync(
            bot: ITelegramBotClient,
            exc: Exception,
            source,
            cancellationToken: CancellationToken 
            )
            =
            $"the update handler caught an exception (the bot is down now?) {exc.GetType()}: {exc.Message}, source={source}"
            |>Log.error|>ignore
            Task.CompletedTask
            //raise exc



module Telegram_service =
    
    let stop_bot
        (bot: TelegramBotClient)
        (cancel_token_source: CancellationTokenSource)
        =
        cancel_token_source.Cancel()
        //bot
    
    let restart_program () =
        "restarting the program"|>Log.important
        Process.Start(Environment.ProcessPath)|>ignore
        Environment.Exit(0)
    
    
    let rec work_resiliently () =
        let bot = TelegramBotClient(Settings.bot_token)
        
        let cancel_token_source = new CancellationTokenSource();
        
        // let receiverOptions = 
        //     ReceiverOptions(
        //         AllowedUpdates = Array.Empty<UpdateType>()
        //     )
        
        try
            bot.ReceiveAsync(
                Update_handler(),
                cancellationToken=cancel_token_source.Token
            )|>Task.WaitAll
        with
        | :? OperationCanceledException  as exc ->
            $"bot was cancelled: {exc.Message}"
            |>Log.important
        | exc ->
            $"""bot threw an exception {exc.GetType()}: {exc.Message}"""
            |>Log.error|>ignore
            //stop_bot bot cancel_token_source
            work_resiliently ()
            //restart_program()
        
