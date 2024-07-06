namespace rvinowise.telegram_defender

open System
open System.Collections
open System.Security
open System.Text.Json
open System.Threading
open System.Threading.Tasks
open FSharp.Data
open JsonExtensions
open Telegram.Bot
open Telegram.Bot.Polling
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups


// module Contacting_bot =
//     
//     let first_interaction
//         (bot: ITelegramBotClient)
//         database
//         (user: User_id)
//         =
//         let groups =
//             Unauthorised_strangers_database.read_groups_in_which_stranger
//                 database
//                 user
//             |>List.ofSeq
//             
//         match groups with
//         | [] -> ()
//         | groups ->
//             groups
//             |>Seq.iter(fun group ->
//                 Asking_questions.start_questioning_stranger_about_group
//                     bot
//                     database
//                     group
//                     user
//             )
                
