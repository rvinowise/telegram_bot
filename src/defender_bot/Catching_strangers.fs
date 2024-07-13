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


module Catching_strangers =
    
    
    let catch_stranger
        (bot: ITelegramBotClient)
        database
        group
        (update: Update)
        =
        let stranger = update.Message.From
        let stranger_id = User_id stranger.Id
        
        Group_gist_database.write_group_title
            database
            group
            update.Message.Chat.Title
        
        User_gist_database.write_user_name
            database
            stranger
        
        Unauthorised_strangers_database.write_joined_stranger
            database group stranger_id
        
        task {
            Seizing_messages.seize_message
                bot
                database
                group
                update.Message
            |>ignore
            
            stranger_id
            |>Executing_jugements.restrict_joined_stranger
                bot group
            |>ignore
        
            Welcoming_strangers.show_examining_button
                bot
                database
                group
                [|stranger|]
            |>ignore
        }
            
        
    let is_message_written_by_user //as opposed to an automatic system message
        (update: Update)
        =
        let message = update.Message
        if message.NewChatMembers |>isNull|>not then
            false
        elif message.LeftChatMember |>isNull|>not then
            false
        else true
    
    let check_new_written_messages
        (bot: ITelegramBotClient)
        database
        (update: Update)
        =
        let group = (Telegram_group.try_group_from_update update).Value //throws, but it should be valid at this point
        let author = User_id update.Message.From.Id
        
        if 
            Ignored_members_database.is_member_ignored
                database
                group
                author
        then
            Task.CompletedTask
        else
            match
                Asking_questions.questioning_final_conclusion_from_database
                    database
                    author
                    group
            with
            |Passed->Task.CompletedTask
            |Failed->
                $"a foe {author} still writes messages"|>Log.error|>ignore
                Task.CompletedTask
            |Stranger|Indecisive ->
                catch_stranger
                    bot
                    database
                    group
                    update
                    
    let check_new_messages
        (bot: ITelegramBotClient)
        database
        (update: Update)
        =
        if (is_message_written_by_user update)  then
            check_new_written_messages
                bot
                database
                update
        else Task.CompletedTask
                
                
