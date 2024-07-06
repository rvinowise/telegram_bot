namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module Unauthorised_strangers_database =
  


    let write_joined_stranger
        (database: SQLiteConnection)
        (group: Group_id)
        (account: User_id)
        =
        database.Query<unit>(
            $"""
            insert or ignore into "{unauthorised_stranger}" 
                (
                    "{unauthorised_stranger.account}", 
                    "{unauthorised_stranger.group}"
                )
            values (
                @account,
                @group
            );
            """,
            {|
                account = account
                group = group
            |}
        )|>ignore
    
    let remove_stranger
        (database: SQLiteConnection)
        (group: Group_id)
        (account: User_id)
        =
        database.Query<unit>(
            $"""
            delete from "{unauthorised_stranger}" 
            where 
                "{unauthorised_stranger.account}" = @account 
                and "{unauthorised_stranger.group}" = @group
            """,
            {|
                account = account
                group = group
            |}
        )|>ignore
    
    let read_groups_in_which_stranger
        (database: SQLiteConnection)
        (account: User_id)
        =
        database.Query<Group_id>(
            $"""
            select "{unauthorised_stranger.group}"
            from "{unauthorised_stranger}"
            where 
                "{question_tried.account}" = @account
            """,
            {|
                account = account
            |}
        )
     