namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables


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
    
    let create_table_unathorised_strangers
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{unauthorised_stranger}" (
                "{unauthorised_stranger.account}" INTEGER,
                "{unauthorised_stranger.group}" INTEGER,
                PRIMARY KEY(
                    "{unauthorised_stranger.account}",
                    "{unauthorised_stranger.group}"
                )
            );
            """
        )|>ignore
        database
        
    let create_table_question_tried
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{question_tried}" (
                "{question_tried.account}" integer,
                "{question_tried.question}" text,
                "{question_tried.group}" integer,
                "{question_tried.answer}" text,
                PRIMARY KEY(
                    "{question_tried.account}",
                    "{question_tried.question}",
                    "{question_tried.group}"
                )
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
    
    let create_table_group_policy
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{group_policy}" (
                "{group_policy.group}" integer PRIMARY KEY,
                "{group_policy.maximum_questions_amount}" integer NOT NULL default 0,
                "{group_policy.minimum_accepted_score}" integer NOT NULL default 0,
                "{group_policy.language}" text NOT NULL default 'Eng'
            );
            """
        )|>ignore
        database
        
    let create_table_group_gist
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{group_gist}" (
                "{group_gist.group}" integer PRIMARY KEY,
                "{group_gist.title}" text NOT NULL default ''
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
    
    
    let create_table_button_callback_data
        (database: SQLiteConnection)
        =
        database.Query<unit>(
            $"""
            CREATE TABLE if not exists "{button_callback_data}" (
                "{button_callback_data.button}" text PRIMARY KEY,
                "{button_callback_data.callback_data}" json
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
        |>create_table_question_tried
        |>create_table_question
        |>create_table_question_answer
        |>create_table_button_callback_data
        |>create_table_group_policy
        |>create_table_group_gist
        |>create_table_unathorised_strangers