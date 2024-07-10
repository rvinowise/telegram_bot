namespace rvinowise.telegram_defender

open System
open System.Globalization
open Telegram.Bot.Types


type User_id = |User_id of int64

module User_id =
    let value (object:User_id) =
        let (User_id value) = object
        value

    let asChatId (user: User_id) =
        ChatId(value user)

[<CLIMutable>]
type Telegram_user_for_db =
    {
        id: User_id
        first_name:string
        last_name:string
        username:string
    }

module Telegram_user_for_db =
    let of_api_user (api_user: User) =
        {
            Telegram_user_for_db.id = User_id api_user.Id
            first_name =  api_user.FirstName
            last_name =
                match api_user.LastName with
                | ""|null -> ""
                | surname -> surname;
            username =
                match api_user.Username with
                | null -> ""
                | username -> $"(@%s{username})"
        }
        
type Telegram_user =
    {
        id: User_id
        first_name:string
        last_name:string option
        username:string option
    }

module Telegram_user =

    let of_db_user (db_user: Telegram_user_for_db) =
        {
            Telegram_user.id = db_user.id
            first_name =  db_user.first_name
            last_name =
                match db_user.last_name with
                | ""|null -> None
                | last_name -> Some last_name;
            username =
                match db_user.username with
                | ""|null -> None
                | username -> Some username
        }
    
    let of_api_user (api_user: User) =
        {
            Telegram_user.id = User_id api_user.Id
            first_name =  api_user.FirstName
            last_name =
                match api_user.LastName with
                | ""|null -> None
                | surname -> Some surname;
            username =
                match api_user.Username with
                | null -> None
                | username -> Some username
        }
    
    let description_from_api_user (user:User) =
        [
            Some user.FirstName;
            match user.LastName with
            | ""|null -> None
            | surname -> Some surname;
            match user.Username with
            | null -> None
            | username -> Some $"(@%s{username})"
        ]
        |>List.choose id
        |>String.concat " "
        
    let description (user: Telegram_user) =
        [
            Some user.first_name;
            user.last_name;
            match user.username with
            | None -> None
            | Some username -> Some $"(@%s{username})"
        ]
        |>List.choose id
        |>String.concat " "
        
    let precise_destription_from_api_user (user:User) =
        let is_premium =
            if (user.IsPremium.GetValueOrDefault(false) = true) then
                "premium"
            else ""
        $"{user.FirstName} {user.LastName} {user.Username} id={user.Id} {is_premium}"