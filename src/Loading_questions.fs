namespace rvinowise.telegram_defender

open System
open System.IO
open System.Text.Json.Nodes
open Microsoft.Extensions.Configuration
open FSharp.Data


type Answer =
    {
        text:string
        score: int
    }

type Question =
    {
        message: string
        answers: Answer list
    }


module Loading_questions =

                
    JsonValue.Load(Settings.config_filename)