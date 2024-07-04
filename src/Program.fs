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
        loading_questions.questions_from_config_to_database() |>ignore
        
        let bot = TelegramBotClient(Settings.bot_token)
        
        //Preparing_commands.prepare_commands bot |>Async.AwaitTask|>ignore
        
        let receiverOptions =
            ReceiverOptions(
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            )
        
        bot.StartReceiving(
            Update_handler(),
            receiverOptions
        )
        
        let user = bot.GetMeAsync()|>Async.AwaitTask
        
        Log.important "bot is running, press any key to cancel"
        Console.ReadLine()|>ignore
                
        
        use cancel_token = new CancellationTokenSource()
        cancel_token.Cancel();
        0