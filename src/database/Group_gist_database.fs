namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.Localised_text
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module Group_gist_database =
    
    
       
    let read_title
        (database: SQLiteConnection)
        (group: Group_id)
        =
        database.Query<string>(
            $"""
            select 
                "{group_gist.title}"
            from "{group_gist}"
            where 
                "{group_gist.group}" = @group
            """,
            {|
                group = group
            |}
        )|>Seq.tryHead
        |>Option.defaultValue ""


    let write_title
        (database: SQLiteConnection)
        (group: Group_id)
        (title: string)
        =
        database.Query<unit>(
            $"""
            insert or replace into "{group_gist}" 
                (
                    "{group_gist.group}",
                    "{group_gist.title}"
                )
            values (
                @group,
                @title
            );
            """,
            {|
                group = group
                title = title
            |}
        )|>ignore
    
    