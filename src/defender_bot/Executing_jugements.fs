namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.Enums
open Telegram.Bot.Types.ReplyMarkups





module Executing_jugements =
    let restrict_joined_stranger
        (bot: ITelegramBotClient)
        group
        stranger
        =
        $"restricting a joined stranger {stranger} in group {group}"|>Log.info
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
            User_id.value stranger,
            muted_permissions)
    
    let lift_restrictions
        (bot: ITelegramBotClient)
        group
        (user)
        =
        $"lifting restrictions from user {user} in group {group}"|>Log.info
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
    
    let group_invite_link 
        title
        url
        =
        $"[{title}]({url})"
        
    
    let announce_passing_test
        (bot: ITelegramBotClient)
        database
        group_id
        user
        language
        group_title
        =
        let all_seized_messages =
            Seized_message_database.retrieve_seized_messages
                database
                group_id
                user
            |>String.concat "\n\n"
        
        
        task {
            let! group = bot.GetChatAsync(Group_id.to_ChatId group_id)
            let group_url =
                group_invite_link
                    group_title
                    group.InviteLink
            
            let combined_message =
                if all_seized_messages <> "" then
                    [|
                        all_seized_messages :> obj
                        group_url :> obj
                    |]
                    |>Localised_text.text_param
                          language
                          Localised_text.answering_success_with_returging_message
                else
                    Localised_text.text_param
                          language
                          Localised_text.answering_success_without_returning_message
                          [|group_url|]
                          
            bot.SendTextMessageAsync(
                (User_id.asChatId user),
                combined_message,
                parseMode = ParseMode.Markdown
            )|>ignore
        }
        
    let make_friend
        bot
        database
        group
        user
        =
        let language =
            Group_policy_database.read_language database group
        
        let group_title =
            Group_gist_database.read_title
                database group
        
        Unauthorised_strangers_database.remove_stranger
            database group user
        
        task {
            lift_restrictions bot group user |>ignore
            
            announce_passing_test
                bot
                database
                group
                user
                language
                group_title
            |>ignore
        }
        
    let make_foe
        (bot: ITelegramBotClient)
        database
        (group: Group_id)
        user
        =
        let language =
            Group_policy_database.read_language database group
        
        let group_title =
            Group_gist_database.read_title
                database group
        
        Unauthorised_strangers_database.remove_stranger
            database group user
        
        task {
            bot.BanChatMemberAsync(
                Group_id.value group, 
                User_id.value user
            )|>ignore
            bot.SendTextMessageAsync(
                (User_id.asChatId user),
                (Localised_text.text_param language Localised_text.answering_fail [|group_title|])
            )|>ignore
        }
        
        
    let announce_failure_in_test
        (bot: ITelegramBotClient)
        database
        (group: Group_id)
        user
        =
        let language =
            Group_policy_database.read_language database group
        
        let group_title =
            Group_gist_database.read_title
                database group
        
        Unauthorised_strangers_database.remove_stranger
            database group user
        
        bot.SendTextMessageAsync(
            (User_id.asChatId user),
            (Localised_text.text_param language Localised_text.answering_fail [|group_title|])
        )
            
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        