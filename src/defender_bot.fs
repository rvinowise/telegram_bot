namespace rvinowise.telegram_defender

open System
open System.Threading
open System.Threading.Tasks
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups





module Questioning_defender =
    
    let user_asked_to_join = "user_asked_to_join"
    
    let user_destription (user:User) =
        let is_premium =
            if (user.IsPremium = true) then
                "premium"
            else ""
        $"{user.FirstName} {user.LastName} {user.Username} id={user.Id} {is_premium}"
    
    
    let restrict_joined_stranger
        (bot: ITelegramBotClient)
        chat
        (stranger: User)
        =
        let muted_permissions =
            ChatPermissions(
                CanSendMessages = false,
                CanSendOtherMessages = false,
                CanChangeInfo = false,
                CanInviteUsers = false,
                CanManageTopics = false,
                CanPinMessages = false,
                CanSendAudios = false,
                CanSendDocuments = false,
                CanSendPhotos = false,
                CanSendPolls = false,
                CanSendVideos = false,
                CanSendVideoNotes = false,
                CanSendVoiceNotes = false,
                CanAddWebPagePreviews = false
            )
        bot.RestrictChatMemberAsync(chat, stranger.Id, muted_permissions)
    
    
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
            |>Array.map
                (restrict_joined_stranger bot chat)
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
        chat
        (users: User array)
        =
        let strangers =
            users
            |>Array.filter (fun user ->
                User_database.is_friend
                    database
                    user.Id
                |>not
            )
    
        handle_joined_strangers bot chat strangers
    
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
                InlineKeyboardButton.WithCallbackData(answer, answer);
            )
            |>Array.ofSeq
            
        bot.SendTextMessageAsync(
            target,
            (question.message),
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
            
            ask_question bot stranger_chat question1
            ask_question bot stranger_chat question2
            
            User_database.add_friend database stranger
            
        }:> Task
    
    let is_stranger
        database
        user
        =
        User_database.is_friend database user
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
            ask_questions_of_stranger bot database user
        else
            dismiss_click bot click


type Update_handler()  =
    
    member this.database = Database.open_connection()
    member this.untested_strangers: List<User> = []
    
    interface IUpdateHandler with
        member this.HandleUpdateAsync(
            bot: ITelegramBotClient ,
            update: Update ,
            cancellationToken: CancellationToken )
            =
            
            if
                (update.CallbackQuery|>isNull|>not) &&
                (update.CallbackQuery.Data = Questioning_defender.user_asked_to_join)
                then
                    Questioning_defender.on_user_asked_to_join
                        bot
                        this.database
                        update.CallbackQuery.From
                        update.CallbackQuery.Id
            elif (isNull update.Message) then
                Task.CompletedTask
            else
                let chatId = ChatId update.Message.Chat.Id
                
                if update.Message.NewChatMembers|>isNull|>not then
                    Questioning_defender.handle_joined_users
                        bot
                        this.database
                        chatId
                        update.Message.NewChatMembers
                    
                else 
                    Task.CompletedTask

              
        member this.HandlePollingErrorAsync(
            bot: ITelegramBotClient,
            exc: Exception,
            cancellationToken: CancellationToken 
            )
            =

            Task.CompletedTask



