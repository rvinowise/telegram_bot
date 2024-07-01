namespace rvinowise.telegram_defender

open System
open System.IO
open System.Text.Json.Nodes
open Microsoft.Extensions.Configuration
open FSharp.Data


module Settings =
    let config_filename =
        Path.Combine
            [|
                DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName
                "appsettings.json"
            |]
    
    let configuration_builder =
        ConfigurationBuilder().
            SetBasePath(DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName).
            AddJsonFile("appsettings.json", false, true);
    let configuration_root = configuration_builder.Build() :> IConfiguration
    
    let bot_token =
            configuration_root.GetValue<string>("bot_token")
                
