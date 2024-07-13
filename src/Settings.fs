namespace rvinowise.telegram_defender

open System
open System.IO
open System.Text.Json.Nodes
open Microsoft.Extensions.Configuration
open FSharp.Data


module Settings =
    
        
    let settings_directory =
        DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName
    let config_filename =
        Path.Combine
            [|
                settings_directory
                "appsettings.json"
            |]
    
    let configuration_builder =
        ConfigurationBuilder().
            SetBasePath(DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName).
            AddJsonFile("appsettings.json", false, true);
    let configuration_root = configuration_builder.Build() :> IConfiguration
    
    let bot_token =
        configuration_root.GetValue<string>("bot_token")
    let bot_url = configuration_root.GetValue<string>("bot_url")
                
    let minimum_accepted_score =
        configuration_root.GetValue<int>("minimum_accepted_score")
        
    let maximum_questions_amount =
        configuration_root.GetValue<int>("maximum_questions_amount")
