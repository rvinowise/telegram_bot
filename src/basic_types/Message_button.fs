namespace rvinowise.telegram_defender

open System
open System.Globalization
open Telegram.Bot.Types



type Message_button =
    {
        text: string
        callback_data: string
    }

module Message_button =
    
    let is_url button =
        button.callback_data.StartsWith("http://")
        ||
        button.callback_data.StartsWith("https://")

    

    