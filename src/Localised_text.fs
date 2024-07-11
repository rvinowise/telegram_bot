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
            Language.Eng, "contact the bot";
            Language.Rus, "сконтактироваться с ботом";
        ]|>Map.ofList

    let welcoming_newcomers =
        [
            Language.Eng, "hi, {0}\nto join, contact the bot by clicking the button below:";
            Language.Rus, "{0},\nчтобы войти, сконтактируйтесь с ботом по кнопке ниже:";
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
            Language.Eng, "answer questions to join the group {0}";
            Language.Rus, "ответьте на вопросы чтобы войти в группу {0}";
        ]|>Map.ofList
    
    let answering_success =
        [
            Language.Eng, "it seems you're not a spamer, here's your deleted message:\n\n{0}\n\nnow you can write in {1}";
            Language.Rus, "похоже вы не спамер, возвращаю ваше удаленное сообщение:\n\n{0}\n\nтеперь вы можете писать в {1}";
        ]|>Map.ofList
    
    let answering_fail =
        [
            Language.Eng, "your answers are wrong for the group {0}";
            Language.Rus, "ваши ответы плохие для группы {0}";
        ]|>Map.ofList
    