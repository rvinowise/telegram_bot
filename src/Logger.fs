namespace rvinowise.telegram_defender

open System.Text.Json
open Serilog
open Xunit

    

module Log =
    let logger =
        LoggerConfiguration()
            .WriteTo.File(
                "../log/log.txt",
                rollOnFileSizeLimit = true, 
                fileSizeLimitBytes = 1000000000
            )
        //#if DEBUG
            .MinimumLevel.Debug()
        //#endif
            .CreateLogger();
    
    let info = logger.Information
    let debug = logger.Debug
    let important message =
        logger.Information message
        printfn $"%s{message}"
    
    let error message =
        logger.Error message
        printfn $"%s{message}"
        message
    
    let json_options =
        let jso = JsonSerializerOptions();
        jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        |>ignore
        jso
        
    let to_json (value:obj) =
        Newtonsoft.Json.JsonConvert.SerializeObject (value, Newtonsoft.Json.Formatting.Indented)