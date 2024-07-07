namespace rvinowise.telegram_defender

open System
open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.Localised_text
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables



module Group_policy_database =
    
    [<CLIMutable>]
    type Questioning_harshness = {
        minimum_accepted_score: int
        maximum_questions_amount: int
    }
    
    let default_questionint_harshness = {
        minimum_accepted_score = 4 
        maximum_questions_amount= 3 
    }
       
    let read_questioning_harshness
        (database: SQLiteConnection)
        (group: Group_id)
        =
        database.Query<Questioning_harshness>(
            $"""
            select 
                "{group_policy.minimum_accepted_score}",
                "{group_policy.maximum_questions_amount}"
            from "{group_policy}"
            where 
                "{group_policy.group}" = @group
            """,
            {|
                group = group
            |}
        )|>Seq.tryHead
        |>function
        |None ->
            $"no questioning harshness set for group {group}, default is used"
            |>Log.error|>ignore
            default_questionint_harshness
        |Some harshness ->
            harshness


    let write_questioning_harshness
        (database: SQLiteConnection)
        (group: Group_id)
        (minimum_accepted_score: int)
        (maximum_questions_amount: int)
        =
        database.Query<unit>(
            $"""
            insert or replace into "{group_policy}" 
                (
                    "{group_policy.group}",
                    "{group_policy.minimum_accepted_score}", 
                    "{group_policy.maximum_questions_amount}"
                )
            values (
                @group,
                @minimum_accepted_score,
                @maximum_questions_amount
            );
            """,
            {|
                group = group
                minimum_accepted_score = minimum_accepted_score
                maximum_questions_amount = maximum_questions_amount
            |}
        )|>ignore
    
    let read_language
        (database: SQLiteConnection)
        (group: Group_id)
        =
        database.Query<Language>(
            $"""
            select 
                "{group_policy.language}"
            from "{group_policy}"
            where 
                "{group_policy.group}" = @group
            """,
            {|
                group = group
            |}
        )|>Seq.tryHead
        |>function
        |None ->
            $"no language set for group {group}, default is used"
            |>Log.error|>ignore
            Language.Eng
        |Some language ->
            language
            
    let write_language
        (database: SQLiteConnection)
        (group: Group_id)
        (language: Language)
        =
        database.Query<unit>(
            $"""
            update "{group_policy}" 
            set "{group_policy.language}" =  @language
            where 
                "{group_policy.group}" = @group
            """,
            {|
                group = group;
                language = language
            |}
        )
        
    let read_time_keeping_strangers
        (database: SQLiteConnection)
        (group: Group_id)
        =
        database.Query<TimeSpan>(
            $"""
            select 
                "{group_policy.how_long_keep_strangers}"
            from "{group_policy}"
            where 
                "{group_policy.group}" = @group
            """,
            {|
                group = group
            |}
        )|>Seq.tryHead
        |>function
        |None ->
            $"no language set for group {group}, default is used"
            |>Log.error|>ignore
            TimeSpan.MaxValue
        |Some timespan ->
            timespan
            
    let write_time_keeping_strangers
        (database: SQLiteConnection)
        (group: Group_id)
        (timespan: TimeSpan)
        =
        database.Query<unit>(
            $"""
            update "{group_policy}" 
            set "{group_policy.how_long_keep_strangers}" =  @timespan
            where 
                "{group_policy.group}" = @group
            """,
            {|
                group = group;
                timespan = timespan
            |}
        )