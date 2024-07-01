namespace rvinowise.telegram_defender.database_schema


type Account_trust_table() =
    override _.ToString() = "account_trust"
    member _.account = "account"
    member _.score = "score"
    member _.questions_amount = "questions_amount"


type Friend_table() =
    override _.ToString() = "friend"
    member _.id = "id"
    member _.name = "name"
    member _.last_name = "last_name"
    member _.username = "username"
    
    
module tables =
    let friend = Friend_table()
    let account_trust = Account_trust_table()