namespace rvinowise.telegram_defender

open System
open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module Ignored_members_database =
  


    let write_ignored_member
        (database: SQLiteConnection)
        (group: Group_id)
        (account: User_id)
        =
        database.Query<unit>(
            $"""
            insert or ignore into "{ignored_member}" 
                (
                    "{ignored_member.account}", 
                    "{ignored_member.group}",
                    "{ignored_member.when_noticed}"
                )
            values (
                @account,
                @group,
                @when_noticed
            );
            """,
            {|
                account = account
                group = group
                when_noticed = DateTime.Now
            |}
        )|>ignore
    
    let remove_ignored_member
        (database: SQLiteConnection)
        (group: Group_id)
        (account: User_id)
        =
        database.Query<unit>(
            $"""
            delete from "{ignored_member}" 
            where 
                "{ignored_member.account}" = @account 
                and "{ignored_member.group}" = @group
            """,
            {|
                account = account
                group = group
            |}
        )|>ignore
    
    let read_groups_in_which_user_is_ignored
        (database: SQLiteConnection)
        (account: User_id)
        =
        database.Query<Group_id>(
            $"""
            select "{ignored_member.group}"
            from "{ignored_member}"
            where 
                "{ignored_member.account}" = @account
            """,
            {|
                account = account
            |}
        )
    
    let is_member_ignored
        (database: SQLiteConnection)
        (group: Group_id)
        (user: User_id)
        =
        database.Query<User_id>(
            $"""
            select "{ignored_member.account}"
            from "{ignored_member}"
            where 
                "{ignored_member.account}" = @account
                and 
                "{ignored_member.group}" = @group
            """,
            {|
                account = user
                group = group
            |}
        )|>Seq.length > 0
            
    
    let read_ignored_members
        (database: SQLiteConnection)
        (group: Group_id)
        =
        database.Query<Group_id>(
            $"""
            select "{ignored_member.account}"
            from "{ignored_member}"
            where 
                "{ignored_member.group}" = @group
            """,
            {|
                group = group
            |}
        )