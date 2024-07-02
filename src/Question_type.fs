namespace rvinowise.telegram_defender


type Question_id = |Question_id of string
type User_id = |User_id of int64
type Group_id = |Group_id of int64

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
    