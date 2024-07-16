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
        Telegram.mute_user
            bot
            group
            stranger
    
    let lift_restrictions
        (bot: ITelegramBotClient)
        group
        (user)
        =
        $"lifting restrictions from user {user} in group {group}"|>Log.info
        
        Telegram.unmute_user
            bot
            group
            user
    
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
            
            return 
                if all_seized_messages <> "" then
                    Localised_text.text_param
                        language
                        Localised_text.answering_success_with_returning_message
                        [|group_url|]
                    |>Telegram.send_message
                        bot
                        (User_id.to_Chat_id user)
                    |>ignore
                    
                    Telegram.send_message
                        bot
                        (User_id.to_Chat_id user)
                        all_seized_messages
                else
                    Localised_text.text_param
                          language
                          Localised_text.answering_success_without_returning_message
                          [|group_url|]
                    |>Telegram.send_message
                        bot
                        (User_id.to_Chat_id user)
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
        
        lift_restrictions bot group user |>ignore
        
        announce_passing_test
            bot
            database
            group
            user
            language
            group_title
        
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
            Telegram.ban_user
                bot
                group
                user
            |>ignore
            
            Telegram.send_message
                bot
                (User_id.to_Chat_id user)
                (Localised_text.text_param language Localised_text.answering_fail [|group_title|])
            |>ignore
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
        
        Telegram.send_message
            bot
            (User_id.to_Chat_id user)
            (Localised_text.text_param language Localised_text.answering_fail [|group_title|])
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        