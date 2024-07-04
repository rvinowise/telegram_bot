namespace rvinowise.telegram_defender

open Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection
open Telegram.Bot
open Telegram.Bot.Types
open Telegram.Bot.Types.ReplyMarkups


module Callback_language =
    
    [<Literal>]
    let action = "action"
    
    [<Literal>]
    let user_asked_to_join = "user_asked_to_join"
    [<Literal>]
    let user_answered_question_about_group = "user_answered_question_about_group"

    [<Literal>]
    let answering_question = "answering_question"
    [<Literal>]
    let question_id = "question_id"
    [<Literal>]
    let about_group = "about_group"
    [<Literal>]
    let given_answer = "given_answer"


module Bot_commands =
    let load_questions_about_group = "load_questions"




module Preparing_commands =
    
    let prepare_commands
        (bot: ITelegramBotClient)
        =
        [
            Bot_commands.load_questions_about_group,
            "reply with json questions to bot's message to load them for questioning about a group"
        ]
        |>List.map(fun (command,description) ->
            BotCommand(
                Command = command,
                Description = description
            )
        )
            
        |>bot.SetMyCommandsAsync



// module Handle_commands =
//     
//     
//         
//     let command_handlers =
//         [
//             Bot_commands.load_questions_about_group,
//             Adding_questions_about_group.ask_for_a_group
//         ]