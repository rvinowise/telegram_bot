namespace rvinowise.telegram_defender

open System
open System.Collections
open System.Security
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups





module Seizing_messages =
    
    
    let important_content_of_message
        (message: Message)
        =
        [
            message.Text
            message.Caption
        ]|>List.tryFind(fun content -> content |> isNull |> not)
        
    
    let seize_message
        (bot: ITelegramBotClient)
        database
        group
        (message: Message)
        =
        
        let author =  message.From
        let message_id = message.MessageId
        let message_text = important_content_of_message message
        
        match message_text with
        |Some message_text ->
        
            $"start seizing a message {message_id} from {author.Id} in group {group}"
            |>Log.info
            
            Seized_message_database.write_seized_message
                database
                group
                (User_id author.Id)
                message_text
        |None ->
            $"message {message_id} from stranger {author.Id} in group {group} doesn't have text, deleting it without saving"
            |>Log.info
        
        Telegram.delete_message
            bot
            (Group_id.to_Chat_id group)
            message_id
    
    let publish_seized_messages
        (bot: ITelegramBotClient)
        database
        group_id
        (author_id: User_id)
        =
        let author_description =
            match
                User_gist_database.read_user_name
                    database
                    author_id
            with
            |Some author ->
                Telegram_user.of_db_user author
                |>Telegram_user.description
            |None -> "somebody"
        
        let all_seized_messages =
            Seized_message_database.retrieve_seized_messages
                database
                group_id
                author_id
            |>String.concat "\n\n"
        
        let combined_message =
            $"""
            {author_description} wrote:\n{all_seized_messages}
            
            """
        Telegram.send_message
            bot
            (Group_id.to_Chat_id group_id)
            combined_message
        
        
        
    
   