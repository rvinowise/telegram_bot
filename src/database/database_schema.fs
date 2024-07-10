namespace rvinowise.telegram_defender.database_schema



    
type Account_score_in_group_table() =
    override _.ToString() = "account_score_in_group"
    member _.account = "account"
    member _.group = "group"
    member _.score = "score"
    member _.questions_amount = "questions_amount"

type Unauthorised_strangers_table() =
    override _.ToString() = "unauthorised_strangers"
    member _.account = "account"
    member _.group = "group"
    member _.when_joined = "when_joined"
 
type Ignored_members_table() =
    override _.ToString() = "ignored_members"
    member _.account = "account"
    member _.group = "group"
    member _.when_noticed = "when_noticed"
 
type Question_answered_table() =
    override _.ToString() = "question_answered"
    member _.account = "account"
    member _.question = "question"
    member _.group = "group"
    member _.answer = "answer"


type Question_table() =
    override _.ToString() = "question"
    member _.id = "id"
    member _.group = "group"
    member _.text = "text"

type Group_policy_table() =
    override _.ToString() = "group_policy"
    member _.group = "group"
    member _.minimum_accepted_score = "minimum_accepted_score"
    member _.maximum_questions_amount = "maximum_questions_amount"
    member _.language = "language"
    member _.how_long_keep_strangers = "how_long_keep_strangers"

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

type Button_callback_data_table() =
    override _.ToString() = "button_callback_data"
    member _.button = "button"
    member _.callback_data = "callback_data"


type Group_gist_table() =
    override _.ToString() = "group_gist"
    member _.group = "group"
    member _.title = "title"

type User_gist_table() =
    override _.ToString() = "user_gist"
    member _.id = "id"
    member _.first_name = "first_name"
    member _.last_name = "last_name"
    member _.username = "username"


type Seized_message_table() =
    override _.ToString() = "seized_message"
    member _.group = "group"
    member _.author = "author"
    member _.text = "text"
    member _.when_seized = "when_written"

module tables =
    let account_score_in_group = Account_score_in_group_table()
    let unauthorised_stranger = Unauthorised_strangers_table()
    let ignored_member = Ignored_members_table()
    let question_tried = Question_answered_table()
    let question = Question_table()
    let group_policy = Group_policy_table()
    let question_answer = Question_answer_table()
    let message_for_adding_questions_to_group = Message_for_adding_questions_to_group_table()
    let button_callback_data = Button_callback_data_table()
    let group_gist = Group_gist_table()
    let seized_message = Seized_message_table()
    let user_gist = User_gist_table()
