namespace rvinowise.telegram_defender

open System
open System.Security
open System.Threading
open System.Threading.Tasks
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups





module Cleaning_from_old_strangers =
    let kick_old_strangers
        (bot: ITelegramBotClient)
        database
        =
        Unauthorised_strangers_database.read_old_strangers
            database
            
        
        
        
        
        
        
        
        
        
        
        
        