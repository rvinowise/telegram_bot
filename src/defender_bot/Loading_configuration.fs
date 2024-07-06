namespace rvinowise.telegram_defender

open System
open System.IO
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.RegularExpressions
open Microsoft.Extensions.Configuration
open FSharp.Data
open Telegram.Bot
open Telegram.Bot.Types.ReplyMarkups




module Loading_configuration =

    let split_errors_and_successes
        (results: Result<'a,'b> list)
        =
        let values =
            results
            |> List.choose (fun r ->
                match r with
                | Result.Ok ok -> Some ok
                | Result.Error _ -> None)

        let err =
            results
            |> List.choose (fun r ->
                match r with
                | Result.Ok _ -> None
                | Result.Error error -> Some error)
        
        err,values
    
        
    let question_from_json
        about_group
        json_question
        =
        match json_question with
        |JsonValue.Record _ ->
            let question_text = json_question.GetProperty("question").AsString()
            
            let answers =
                json_question["answers"].AsArray()
                |>Array.map(fun json_answer ->
                    {
                        Answer.text = json_answer["text"].AsString()
                        Answer.score = json_answer["score"].AsInteger()
                    }
                )
            {
                Question.text = question_text
                group = about_group
                answers = List.ofArray answers 
            }
            |>Result.Ok
        |_ ->
            "a question should be a json record"
            |>Result.Error
    
    let questions_from_json
        about_group
        json_questions
        =
        match JsonValue.TryParse(json_questions) with
        |Some json_questions ->
            match json_questions with
            |JsonValue.Array json_questions ->
                let (errors,questions) =
                    json_questions
                    |>Array.map(fun json_question ->
                        question_from_json
                            about_group
                            json_question
                    )|>List.ofArray
                    |>split_errors_and_successes
                    
                match errors with
                | error::tail ->
                    Result.Error error
                | [] ->
                    Result.Ok questions
                    
            |_ ->
                "json questions should be embedded into an array"
                |>Result.Error
        |None ->
            "can't parse json"
            |>Result.Error
    
                
    let load_questions
        database
        (file: JsonValue) =
        
        let group = file.GetProperty("group").AsInteger64() |> Group_id
        
        let questions = 
            match file.GetProperty("questions") with
            |JsonValue.Array questions ->
                questions_from_json
                    group
                    (file.GetProperty("questions").ToString())
            |_ ->
                "there should be an array of 'questions' in the configuration file"
                |>JsonException
                |>raise
            
        questions
        |>Result.map(fun questions ->
            questions
            |>List.iter (Question_database.write_question database)
        )
        |>Result.mapError(fun error ->
            Log.error $"error loading questions from json: {error}"
        )
        
    let load_group_policy
        database
        (file: JsonValue)
        =
        let group = file.GetProperty("group").AsInteger64() |> Group_id
        let minimum_accepted_score = file.GetProperty("minimum_accepted_score").AsInteger()
        let maximum_questions_amount = file.GetProperty("maximum_questions_amount").AsInteger()
        Group_policy_database.write_questioning_harshness
            database
            group
            minimum_accepted_score
            maximum_questions_amount
            
        let language = file.GetProperty("language").AsString() |> Language.from_string
        Group_policy_database.write_language
            database
            group
            language
        
    let load_configuration_to_database() =
        let file = JsonValue.Load(Settings.config_filename)
        let database = Database.open_connection()
        
        load_group_policy
            database file
        |>ignore
            
        load_questions
            database file
        |>ignore
    
    
module Adding_questions_about_group =
    let ask_for_a_group
        (bot: ITelegramBotClient)
        (user: User_id)
        =
        let button_parameters =
            KeyboardButtonRequestChat(
                RequestId = 666,
                ChatIsChannel = false
            )
        let button = KeyboardButton.WithRequestChat("select a group",button_parameters)
        
        bot.SendTextMessageAsync(
            chatId=User_id.asChatId user,
            text="select a group!",
            replyMarkup = ReplyKeyboardMarkup(button)
        )
    
    let ask_for_json_questions
        (bot: ITelegramBotClient)
        user
        (about_group)
        =
        bot.SendTextMessageAsync(
            chatId=User_id.asChatId user,
            text="reply to this message with json questions"
        )
    
    let is_message_replying_to_asking_for_json_questions
        (bot: ITelegramBotClient)
        =()
    
            
    let try_group_for_adding_questions_from_message
        (bot: ITelegramBotClient)
        message
        =
        ()