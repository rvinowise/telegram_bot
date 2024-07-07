namespace rvinowise.telegram_defender

open System
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
                    "{unauthorised_stranger.group}",
                    "{unauthorised_stranger.when_joined}"
                )
            values (
                @account,
                @group,
                @when_joined
            );
            """,
            {|
                account = account
                group = group
                when_joined = DateTime.Now
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
    
    let read_groups_in_which_user_is_stranger
        (database: SQLiteConnection)
        (account: User_id)
        =
        database.Query<Group_id>(
            $"""
            select "{unauthorised_stranger.group}"
            from "{unauthorised_stranger}"
            where 
                "{unauthorised_stranger.account}" = @account
            """,
            {|
                account = account
            |}
        )
    
    let read_old_strangers
        (database: SQLiteConnection)
        (older_than: TimeSpan)
        =
        database.Query<Group_id>(
            $"""
            select "{unauthorised_stranger.account}","{unauthorised_stranger.group}"
            from "{unauthorised_stranger}"
            where 
                "{unauthorised_stranger.when_joined}" < @min_date
            """,
            {|
                min_date = DateTime.Now - older_than
            |}
        )