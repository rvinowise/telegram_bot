namespace rvinowise.telegram_defender.database_schema



    
type Account_score_in_group_table() =
    override _.ToString() = "account_score_in_group"
    member _.account = "account"
    member _.group = "group"
    member _.score = "score"
    member _.questions_amount = "questions_amount"
    
type Question_asked_table() =
    override _.ToString() = "question_asked"
    member _.account = "account"
    member _.question = "question"
    member _.group = "group"
    member _.answer = "answer"


type Question_table() =
    override _.ToString() = "question"
    member _.id = "id"
    member _.group = "group"
    member _.text = "text"

type Question_answer_table() =
    override _.ToString() = "question_answer"
    member _.question = "question"
    member _.group = "group"
    member _.text = "text"
    member _.score = "score"

type Message_for_adding_questions_to_group_table() =
    override _.ToString() = "message_for_adding_questions_to_group"
    member _.message = "message"
    member _.group = "group"

module tables =
    let account_score_in_group = Account_score_in_group_table()
    let question_tried = Question_asked_table()
    let question = Question_table()
    let question_answer = Question_answer_table()
    let message_for_adding_questions_to_group = Message_for_adding_questions_to_group_table()