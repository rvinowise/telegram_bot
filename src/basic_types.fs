namespace rvinowise.telegram_defender

open System
open Telegram.Bot.Types

type Bot_exception(message: string) =
    inherit System.Exception(message)
    
type Question_id = |Question_id of string

module Question_id =
    let value (object:Question_id) =
        let (Question_id value) = object
        value

type User_id = |User_id of int64

module User_id =
    let value (object:User_id) =
        let (User_id value) = object
        value

    let asChatId (user: User_id) =
        ChatId(value user)

type Group_id = |Group_id of int64

module Group_id =
    let value (object:Group_id) =
        let (Group_id value) = object
        value

    let asChatId (group: Group_id) =
        ChatId(value group)

    let try_from_chat (chat: ChatId) =
        if chat.Identifier.HasValue then
            chat.Identifier.Value
            |>Group_id
            |>Some
        else
            None
    
    let from_chat (chat: ChatId) =
        if chat.Identifier.HasValue then
            chat.Identifier.Value
            |>Group_id
        else
            $"chat {chat} doesn't have a numerical id"
            |>Bot_exception
            |>raise
            
    let try_from_string_chat (chat: string) =
        chat
        |>ChatId
        |>try_from_chat
        
            
    let from_string_chat (chat: string) =
        chat
        |>ChatId
        |>from_chat


type Button_id = Button_id of string

module Button_id =
    let value (object:Button_id) =
        let (Button_id value) = object
        value

    let create () =
        Guid.NewGuid()|>string |> Button_id    
    // let to_string (button: Button_id) =
    //     button|>value|>string
        
    

[<CLIMutable>]
type Answer =
    {
        text:string
        score: int
    }

type Question =
    {
        text: string
        group: Group_id
        answers: Answer list
    }
    
module Question =
    let primary_key (question:Question) =
        question.answers
        |>List.map _.text
        |>List.sort
        |>String.concat "#"
        |>sprintf "%s#%s" question.text
        |>Question_id
    
    let answer_score
        (question:Question) 
        answer_text
        =
        let answer =
            question.answers
            |>List.find (fun answer -> answer.text = answer_text)
        answer.score

module Nullable=
    let to_option (n : System.Nullable<_>) = 
        if n.HasValue then
            Some n.Value 
        else None      