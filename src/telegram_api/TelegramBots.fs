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
open Telegram.Bot.Types.Enums
open Telegram.Bot.Types.ReplyMarkups


module Message_button_api =
    let to_api_button
        (button: Message_button)
        =
        if Message_button.is_url button then
            InlineKeyboardButton.WithUrl(
                button.text,
                button.callback_data
            );
        else
            InlineKeyboardButton.WithCallbackData(
                button.text,
                button.callback_data
            )
        
        
    
//https://github.com/TelegramBots/Telegram.Bot
module Telegram =
    
    let send_message
        (bot: ITelegramBotClient)
        (chat: Chat_id)
        message
        =
        bot.SendTextMessageAsync(
            (Chat_id.value chat),
            message,
            parseMode = ParseMode.Markdown
        )
   
    let send_message_with_buttons
        (bot: ITelegramBotClient)
        (chat: Chat_id)
        message
        (buttons: Message_button seq seq)
        =
        let buttons =
            buttons
            |>Seq.map(fun row ->
                row|>Seq.map(fun button ->
                    Message_button_api.to_api_button button
                )
            )
            |>Seq.map (fun row -> Array.ofSeq row :> Generic.IEnumerable<InlineKeyboardButton>)
            |>Array.ofSeq

        bot.SendTextMessageAsync(
            (Chat_id.value chat),
            message,
            parseMode = ParseMode.Markdown,
            replyMarkup=InlineKeyboardMarkup(buttons)
        )
        
    let delete_message
        (bot: ITelegramBotClient)
        (chat: Chat_id)
        message
        =    
        bot.DeleteMessageAsync(Chat_id.value chat, message)
        
    let get_admins
        (bot: ITelegramBotClient)
        (group: Group_id)
        =    
        bot.GetChatAdministratorsAsync(Group_id.to_ChatId group)
        
    let ban_user
        (bot: ITelegramBotClient)
        (group: Group_id)
        (user: User_id)
        =
        bot.BanChatMemberAsync(
            Group_id.value group, 
            User_id.value user
        )
        
    let mute_user
        (bot: ITelegramBotClient)
        (group: Group_id)
        (user: User_id)
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
        bot.RestrictChatMemberAsync(
            Group_id.value group,
            User_id.value user,
            muted_permissions
        )
        
    let unmute_user
        (bot: ITelegramBotClient)
        (group: Group_id)
        (user: User_id)
        =
        let regular_permissions =
            ChatPermissions(
                CanSendMessages = true,
                CanSendOtherMessages = true,
                CanChangeInfo = false,
                CanInviteUsers = true,
                CanManageTopics = false,
                CanPinMessages = false,
                CanSendAudios = true,
                CanSendDocuments = true,
                CanSendPhotos = true,
                CanSendPolls = true,
                CanSendVideos = true,
                CanSendVideoNotes = true,
                CanSendVoiceNotes = true,
                CanAddWebPagePreviews = true
            )
        bot.RestrictChatMemberAsync(
            Group_id.value group,
            User_id.value user,
            regular_permissions
        )
    
    let answer_callback
        (bot: ITelegramBotClient)
        callback
        message
        =
        bot.AnswerCallbackQueryAsync(
            callback,
            message            
        )