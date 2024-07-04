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

    let open_connection () =
        let database = new SQLiteConnection (connection_string "./telegram_defender.db")
        
        Telegram_database_type_mappers.set_telegram_type_handlers()
        
        database



module Creating_database =
    
        
    let create_table_account_score_in_group
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{account_score_in_group}" (
                "{account_score_in_group.account}" INTEGER,
                "{account_score_in_group.group}" INTEGER,
                "{account_score_in_group.score}" INTEGER NOT NULL,
                "{account_score_in_group.questions_amount}" INTEGER NOT NULL,
                PRIMARY KEY(
                    "{account_score_in_group.account}",
                    "{account_score_in_group.group}"
                )
            );
            """
        )|>ignore
        database
        
    let create_table_question_asked
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{question_tried}" (
                "{question_tried.account}" INTEGER PRIMARY KEY,
                "{question_tried.question}" text NOT NULL,
                "{question_tried.answer}" text NOT NULL
            );
            """
        )|>ignore
        database
        
        
    let create_table_question
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{question}" (
                "{question.id}" text,
                "{question.group}" integer,
                "{question.text}" text NOT NULL,
                PRIMARY KEY(
                    "{question.id}",
                    "{question.group}"
                )
            );
            """
        )|>ignore
        database
        
    let create_table_question_answer
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{question_answer}" (
                "{question_answer.question}" text,
                "{question_answer.group}" text,
                "{question_answer.text}" text,
                "{question_answer.score}" integer NOT NULL,
                PRIMARY KEY(
                    "{question_answer.question}",
                    "{question_answer.group}",
                    "{question_answer.text}"
                )
            );
            """
        )|>ignore
        database
    
    // let create_table_message_for_adding_questions_to_group
    //     (database: SQLiteConnection)
    //     =
    //     database.Query<unit>(
    //         $"""
    //         CREATE TABLE if not exists "{message_for_adding_questions_to_group}" (
    //             "{question_answer.question}" text,
    //             "{question_answer.text}" text,
    //             "{question_answer.score}" integer NOT NULL,
    //             PRIMARY KEY(
    //                 "{question_answer.question}",
    //                 "{question_answer.text}"
    //             )
    //         );
    //         """
    //     )|>ignore
    //     database
        
    let ensure_database_created () =
        Database.open_connection ()
        |>create_table_account_score_in_group
        |>create_table_question_asked
        |>create_table_question
        |>create_table_question_answer