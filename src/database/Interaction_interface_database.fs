namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables

module Interaction_interface_database =
    
        
    let read_account_score_in_group
        (database: SQLiteConnection)
        (account: User_id)
        (group: Group_id)
        =
        database.Query<int*int>(
            $"""
            select "{account_score_in_group.questions_amount}","{account_score_in_group.score}"
            from "{account_score_in_group}"
            where 
                "{account_score_in_group.account}" = @account
                and 
                "{account_score_in_group.group}" = @group
            """,
            {|
                account = account
                group = group
            |}
        )|>Seq.tryHead
        |>Option.defaultValue (0,0)


        
    
    let write_account_questioning_result
        (database: SQLiteConnection)
        (account)
        (group)
        (score_change: int)
        =
        let answered_questions,previous_score =
            read_account_score_in_group
                database
                account
                group
        
        database.Query<string>(
            $"""
            insert or replace into "{account_score_in_group}" 
                (
                    "{account_score_in_group.account}", 
                    "{account_score_in_group.group}", 
                    "{account_score_in_group.questions_amount}", 
                    "{account_score_in_group.score}"
                )
            values (
                @account,
                @group,
                @answered_questions_amount,
                @score
            );
            """,
            {|
                account = account
                group = group
                score = previous_score + score_change
                answered_questions_amount = answered_questions + 1
            |}
        )|>ignore

