namespace rvinowise.telegram_defender

open System
open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module Seized_message_database =


    let write_seized_message
        (database: SQLiteConnection)
        (group: Group_id)
        (author: User_id)
        message_text
        =
        database.Query<unit>(
            $"""
            insert or ignore into "{seized_message}" 
                (
                    "{seized_message.group}",
                    "{seized_message.author}", 
                    "{seized_message.text}",
                    "{seized_message.when_seized}"
                )
            values (
                @group,
                @author,
                @text,
                @when_seized
            );
            """,
            {|
                group = group
                author = author
                text = message_text
                when_seized = DateTime.Now
            |}
        )|>ignore
    
    [<CLIMutable>]
    type Seized_message = {
        text: string
        when_seized: DateTime
    }
    
    let read_seized_messages
        (database: SQLiteConnection)
        (group: Group_id)
        (author: User_id)
        =
        database.Query<Seized_message>(
            $"""
            select 
                "{seized_message.text}",
                "{seized_message.when_seized}"
            from "{seized_message}"
            where 
                "{seized_message.author}" = @author
                and
                "{seized_message.group}" = @group
            """,
            {|
                author = author
                group = group
            |}
        )|>Seq.sortBy(_.when_seized)
        |>Seq.map(_.text)
    
    let forget_seized_messages
        (database: SQLiteConnection)
        (group: Group_id)
        (author: User_id)
        =
        database.Query<unit>(
            $"""
            delete from "{seized_message}" 
            where 
                "{seized_message.author}" = @author 
                and "{seized_message.group}" = @group
            """,
            {|
                author = author
                group = group
            |}
        )|>ignore
        
    let retrieve_seized_messages
        (database: SQLiteConnection)
        (group: Group_id)
        (author: User_id)
        =
        let messages =
            read_seized_messages
                database
                group
                author
                
        forget_seized_messages
            database
            group
            author
            
        messages