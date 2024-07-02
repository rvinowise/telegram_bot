namespace rvinowise.telegram_defender

open System
open System.IO
open System.Text.Json
open System.Text.Json.Nodes
open System.Text.RegularExpressions
open Microsoft.Extensions.Configuration
open FSharp.Data




module loading_questions =

                
    let questions_from_config () =
        
        
        let file = JsonValue.Load(Settings.config_filename)
        
        let group_id = file.GetProperty("group").AsInteger64() |> Group_id
        
        match file.GetProperty("questions") with
        |JsonValue.Array questions ->
            questions
            |>Array.map(fun json_question ->
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
                        group = group_id
                        answers = List.ofArray answers 
                    }
                |_ ->
                    "a question should be a json record"
                    |>JsonException
                    |>raise
            )
        |_ ->
            "there should be an array of 'questions' in the configuration file"
            |>JsonException
            |>raise
    
    