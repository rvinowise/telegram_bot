namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.Localised_text
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module User_gist_database =
    
    
       
    let read_user_name
        (database: SQLiteConnection)
        (user_id: User_id)
        =
        database.Query<Telegram_user_for_db>(
            $"""
            select 
                "{user_gist.first_name}",
                "{user_gist.last_name}",
                "{user_gist.username}"
            from "{user_gist}"
            where 
                "{user_gist.id}" = @user_id
            """,
            {|
                user_id = user_id
            |}
        )|>Seq.tryHead


    let write_user_name
        (database: SQLiteConnection)
        (user: User)
        =
        let user_db =
            Telegram_user_for_db.of_api_user user
        database.Query<unit>(
            $"""
            insert or replace into "{user_gist}" 
                (
                    "{user_gist.id}",
                    "{user_gist.first_name}",
                    "{user_gist.last_name}",
                    "{user_gist.username}"
                )
            values (
                @id,
                @first_name,
                @last_name,
                @username
            );
            """,
            {|
                id = user_db.id
                first_name = user_db.first_name
                last_name = user_db.last_name
                username = user_db.username
            |}
        )|>ignore
    
    