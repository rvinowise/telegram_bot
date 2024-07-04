namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables

module Question_database =
    
        
    let read_account_score_in_group
        (database: SQLiteConnection)
        (account)
        group
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


    let write_question_body
        (database: SQLiteConnection)
        (question: Question)
        =
        database.Query<string>(
            $"""
            insert or replace into "{tables.question}" 
                (
                    "{tables.question.id}", 
                    "{tables.question.group}", 
                    "{tables.question.text}" 
                )
            values (
                @id,
                @group,
                @text
            );
            """,
            {|
                id = Question.primary_key question
                group = question.group
                text = question.text
            |}
        )|>ignore
    
    let write_question_answer
        (database: SQLiteConnection)
        (question: Question)
        (answer: Answer)
        =
        database.Query<string>(
            $"""
            insert or replace into "{tables.question_answer}" 
                (
                    "{tables.question_answer.question}", 
                    "{tables.question_answer.group}", 
                    "{tables.question_answer.text}", 
                    "{tables.question_answer.score}"
                )
            values (
                @question,
                @group,
                @answer,
                @score
            );
            """,
            {|
                question = Question.primary_key question
                group = question.group
                answer = answer.text
                score = answer.score
            |}
        )|>ignore
    
    let write_question
        (database: SQLiteConnection)
        (question: Question)
        =
        write_question_body
            database
            question
        
        question.answers
        |>Seq.iter(fun answer ->
            write_question_answer
                database
                question
                answer
        )
        
    let read_question_body
        (database: SQLiteConnection)
        (question: Question_id)
        =
        database.Query<string>(
            $"""
            select "{tables.question.text}"
            from "{tables.question}"
            where 
                "{tables.question.id}" = @question
            """,
            {|
                question = question
            |}
        )|>Seq.head
    
    let read_question_answers
        (database: SQLiteConnection)
        (question: Question_id)
        group
        =
        database.Query<Answer>(
            $"""
            select "{tables.question_answer.text}", "{tables.question_answer.score}"
            from "{tables.question_answer}"
            where 
                "{tables.question_answer.question}" = @question
                and
                "{tables.question_answer.group}" = @group
            """,
            {|
                question = question
                group = group
            |}
        )
    
    let read_question
        (database: SQLiteConnection)
        (group: Group_id)
        (question: Question_id)
        =
        {
            Question.text =
                read_question_body
                    database
                    question
            group = group 
            answers =
                read_question_answers
                    database
                    question
                    group
                |>List.ofSeq
        }