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
    let config_filename =
        Path.Combine
            [|
                DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName
                "appsettings.json"
            |]
    
    // let configuration_builder =
    //     ConfigurationBuilder().
    //         SetBasePath(DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName).
    //         AddJsonFile("appsettings.json", false, true);
    // let configuration_root = configuration_builder.Build() :> IConfiguration
    //
    // let questions_section =
    //             configuration_root.GetSection("questions")
                
    JsonValue.Load(config_filename)