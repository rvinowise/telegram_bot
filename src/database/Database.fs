namespace rvinowise.telegram_defender

open System.IO
open Dapper
open System.Data.SQLite
open Telegram.Bot.Types
open rvinowise.telegram_defender.database_schema
open rvinowise.telegram_defender.database_schema.tables


module Database =
    let connection_string (dataSource: string) =
        sprintf
            "Data Source = %s;"
            dataSource

    let open_connection () =
        let database =
            let connection_string =
                [|
                    Settings.settings_directory
                    "telegram_defender.db"
                |]
                |>Path.Combine
                |>connection_string
            new SQLiteConnection(connection_string)
        
        Telegram_database_type_mappers.set_telegram_type_handlers()
        
        database



