namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups





module Executing_jugements =
    let restrict_joined_stranger
        (bot: ITelegramBotClient)
        chat
        (stranger)
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
        bot.RestrictChatMemberAsync(chat, stranger, muted_permissions)
    
    let lift_restrictions
        (bot: ITelegramBotClient)
        chat
        (user)
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
        let (User_id user_value) = user
        bot.RestrictChatMemberAsync(chat,  user_value, regular_permissions)
        
    let make_friend
        bot
        chat
        user
        =
        lift_restrictions bot chat user
        
    let make_foe
        (bot: ITelegramBotClient)
        chat
        user
        =
        let (User_id user_id_value) = user
        bot.BanChatMemberAsync(chat, user_id_value)
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        