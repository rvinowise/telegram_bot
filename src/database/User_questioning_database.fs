namespace rvinowise.telegram_defender

open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module User_questioning_database =
    
    [<CLIMutable>]
    type Account_score_in_group = {
        questions_amount: int
        score: int
    }    
       
    let read_account_score_in_group
        (database: SQLiteConnection)
        (account: User_id)
        (group: Group_id)
        =
        database.Query<Account_score_in_group>(
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
        |>Option.map(fun record -> record.questions_amount, record.score)
        |>Option.defaultValue (0,0)


      
    
    let change_account_questioning_score
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
        let new_score = previous_score + score_change
        let answered_questions_amount = answered_questions + 1
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
                answered_questions_amount = answered_questions_amount
                score = new_score
            |}
        )|>ignore
        answered_questions_amount,new_score


    let write_asked_question
        (database: SQLiteConnection)
        (account: User_id)
        (question:Question)
        =
        database.Query<unit>(
            $"""
            insert or replace into "{question_tried}" 
                (
                    "{question_tried.account}", 
                    "{question_tried.question}", 
                    "{question_tried.group}"
                )
            values (
                @account,
                @question,
                @group
            );
            """,
            {|
                account = account
                group = question.group
                question = Question.primary_key question
            |}
        )|>ignore
    
    let write_given_answer_to_question
        (database: SQLiteConnection)
        (account: User_id)
        (question:Question)
        answer_id
        =
        database.Query<unit>(
            $"""
            update "{question_tried}" 
            set "{question_tried.answer}" = @answer
            where 
                "{question_tried.account}" = @account
                and "{question_tried.group}" = @group
                and "{question_tried.question}" = @question
            """,
            {|
                account = account
                group = question.group
                question = Question.primary_key question
                answer = answer_id
            |}
        )|>ignore
    
    let read_tried_questions
        (database: SQLiteConnection)
        (account: User_id)
        (group)
        =
        database.Query<Question_id>(
            $"""
            select "{question_tried.question}"
            from "{question_tried}"
            where 
                "{question_tried.account}" = @account
                and 
                "{question_tried.group}" = @group
            """,
            {|
                account = account
                group = group
            |}
        )
        
    let read_group_questions
        (database: SQLiteConnection)
        (group)
        =
        database.Query<Question_id>(
            $"""
            select "{question.id}"
            from "{question}"
            where 
                "{question.group}" = @group
            """,
            {|
                group = group
            |}
        )
        