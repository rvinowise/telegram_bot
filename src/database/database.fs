namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables


module Database =
    let connection_string (dataSource: string) =
        sprintf
            "Data Source = %s;"
            dataSource

    let open_connection () = new SQLiteConnection (connection_string "./telegram_defender.db")




module Create_database =
    
        
    let create_table_account_score_in_group
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists '{account_score_in_group}' (
                '{account_score_in_group.account}' INTEGER PRIMARY KEY,
                '{account_score_in_group.group}' INTEGER PRIMARY KEY,
                '{account_score_in_group.score}' INTEGER NOT NULL,
                '{account_score_in_group.questions_amount}' INTEGER NOT NULL,
            );
            """
        )|>ignore
        database
        
    let create_table_question_asked
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists '{question_asked}' (
                '{question_asked.account}' INTEGER PRIMARY KEY,
                '{question_asked.question}' text NOT NULL,
                '{question_asked.answer}' text NOT NULL,
            );
            """
        )|>ignore
        database
        
        
    let create_table_question
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists '{question}' (
                '{question.id}' text PRIMARY KEY,
                '{question.group}' integer PRIMARY KEY,
                '{question.text}' text NOT NULL,
            );
            """
        )|>ignore
        database
        
    let create_table_question_answer
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists '{question_answer}' (
                '{question_answer.question}' text PRIMARY KEY,
                '{question_answer.text}' text PRIMARY KEY,
                '{question_answer.score}' integer NOT NULL,
            );
            """
        )|>ignore
        database
        
    let provide_database () =
        Database.open_connection ()
        |>create_table_account_score_in_group
        |>create_table_question_asked
        |>create_table_question_asked