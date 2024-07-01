namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema.tables


module Database =
    let connection_string (dataSource: string) =
        sprintf
            "Data Source = %s;"
            dataSource

    let open_connection () = new SQLiteConnection (connection_string "./telegram_defender.db")


module User_database =
    let is_friend
        (database: SQLiteConnection)
        user
        =
        database.Query<string>(
            $"""
            select 'name'
            from 'friend'
            where id = @user
            """,
            {|
                user = user
            |}
        )|>Seq.length > 0
        
    let add_friend
        (database: SQLiteConnection)
        (user: User)
        =
        database.Query<string>(
            $"""
            insert or replace into 'friend' ('id', 'name', 'last_name', 'username')
            values (@id, @name, @last_name, @username);
            """,
            {|
                id = user.Id
                name = user.FirstName
                last_name = user.LastName
                username = user.Username
            |}
        )|>ignore

    let read_account_questioning_result
        (database: SQLiteConnection)
        (account: User)
        =
        database.Query<int*int>(
            $"""
            select '{account_trust.questions_amount}','{account_trust.score}'
            from '{account_trust}'
            where '{account_trust.account}' = @account
            """,
            {|
                account = account
            |}
        )|>Seq.tryHead
        |>Option.defaultValue (0,0)


    let write_account_questioning_result
        (database: SQLiteConnection)
        (account: User)
        (score_change: int)
        =
        let answered_questions,previous_score =
            read_account_questioning_result
                database
                account
        
        database.Query<string>(
            $"""
            insert or replace into '{account_trust}' 
                (
                    '{account_trust.account}', 
                    '{account_trust.questions_amount}', 
                    '{account_trust.score}'
                )
            values (
                @account,
                @answered_questions_amount,
                @score
            );
            """,
            {|
                account = account.Id
                score = previous_score + score_change
                answered_questions_amount = answered_questions + 1
            |}
        )|>ignore


module Create_database =
    
    let create_table_friends
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists friend (
                id INTEGER PRIMARY KEY,
                name TEXT default "" NOT NULL,
                last_name TEXT NOT NULL,
                username TEXT default "" NOT NULL
            );
            """
        )
    
    let create_table_account_trust
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists '{account_trust}' (
                '{account_trust.account}' INTEGER PRIMARY KEY,
                '{account_trust.score}' INTEGER NOT NULL,
                '{account_trust.questions_amount}' INTEGER NOT NULL,
            );
            """
        )
    
    let provide_database () =
        Database.open_connection ()
        |>create_table_friends