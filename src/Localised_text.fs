namespace rvinowise.telegram_defender


type Language =
    |Rus
    |Eng
    
module Language =
    let to_string (language: Language) =
        match language with
        |Eng ->
            "Eng"
        |Rus ->
            "Rus"

    let from_string (code: string) =
        match code with
        |"Eng" ->
            Eng
        |"Rus" ->
            Rus
        |unknown ->
            $"unknown language from database: {unknown}, returning the default"
            |>Log.error|>ignore
            Eng
    
module Localised_text =
    
    let format
        template
        values
        =
        System.String.Format(template, values)
    
    let text
        (language: Language)
        (translations: Map<Language,string>)
        =
        translations
        |>Map.find language
    
    let text_param
        (language: Language)
        (translations: Map<Language,string>)
        (parameters: obj array)
        =
        let template = 
            translations
            |>Map.find language
        format template parameters
    
    let contact_defender_bot =
        [
            Language.Eng, "✅ START THE TEST ✅";
            Language.Rus, "✅ НАЧАТЬ ТЕСТ ✅";
        ]|>Map.ofList

    let welcoming_newcomers =
        [
            Language.Eng, "hi, {0}\nto write here, contact the bot by clicking the button below:";
            Language.Rus, "Привет, {0}!\nЯ защищаю этот чат от спама. Чтобы здесь писать, вам нужно авторизоваться, ответив на несколько вопросов внутри бота.";
        ]|>Map.ofList
    let proceed_to_answering =
        [
            Language.Eng, "respond to a bot asking you questions in private";
            Language.Rus, "ответьте боту на его вопросы в приватных сообщениях";
        ]|>Map.ofList
    let dismissing_click =
        [
            Language.Eng, "you don't need to click this button";
            Language.Rus, "вам не нужно нажимать на эту кнопку";
        ]|>Map.ofList
    
    let answer_questions_prompt =
        [
            Language.Eng, "answer the questions to participate in group {0}";
            Language.Rus, "ответьте на вопросы чтобы общаться в группе {0}";
        ]|>Map.ofList
    
    let answering_success_with_returning_message =
        [
            Language.Eng, "it seems you're not a spamer, now you can write in {0}\nI return your deleted message below";
            Language.Rus, "похоже вы не спамер, теперь вы можете писать в {0}\nвозвращаю ваше удаленное сообщение ниже";
        ]|>Map.ofList
   
    let answering_success_without_returning_message =
        [
            Language.Eng, "it seems you're not a spamer, now you can write in {0}";
            Language.Rus, "похоже вы не спамер, теперь вы можете писать в {0}";
        ]|>Map.ofList
     
    let answering_fail =
        [
            Language.Eng, "your answers are wrong for the group {0}";
            Language.Rus, "ваши ответы плохие для группы {0}";
        ]|>Map.ofList
    