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
        Create_database.provide_database ()
        
        let bot = TelegramBotClient(Settings.bot_token)
        
        let receiverOptions =
            ReceiverOptions(
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            )
        
        bot.StartReceiving(
            Update_handler(),
            receiverOptions
        )
        
        let unitAsync =
            async {
                let! user = bot.GetMeAsync()|>Async.AwaitTask
                printf $"Start listening for @{user.Username}"
            }
        
        Console.ReadLine()
        
        use cancel_token = new CancellationTokenSource()
        cancel_token.Cancel();
        0