namespace rvinowise.telegram_defender


open System
open System.Threading
open System.Threading.Tasks
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.Enums
open Telegram.Bot.Types.ReplyMarkups
open Dapper

module Program =


    [<EntryPoint>]
    let main(args: string[]) =
        Log.info "bot's program has started"
        Creating_database.ensure_database_created () |>ignore
        Loading_configuration.load_configuration_to_database() |>ignore
        
        let result = Telegram_service.start()
        
        
        Log.important "bot is running, enter any key to cancel"
        Console.ReadLine()|>ignore
                
        
        
        0