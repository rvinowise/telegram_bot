namespace rvinowise.telegram_defender

open System
open System.Data
open System.Data.SqlTypes
open Dapper
open rvinowise.telegram_defender
open rvinowise.telegram_defender.Localised_text


      
type Timestamp_mapper() =
    
    inherit SqlMapper.TypeHandler<DateTime>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            datetime_value: DateTime
        )
        =
        parameter.Value <- Timestamp.to_mysql_string datetime_value
    
    override this.Parse(value: obj) =
        let retrieved_datetime =
            value :?> string
        Timestamp.from_mysql_string retrieved_datetime

type Question_id_mapper() =
    inherit SqlMapper.TypeHandler<Question_id>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            value: Question_id
        )
        =
        parameter.Value <- Question_id.value value
    
    override this.Parse(value: obj) =
        Question_id (value :?> string) 

type User_id_mapper() =
    inherit SqlMapper.TypeHandler<User_id>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            value: User_id
        )
        =
        parameter.Value <- User_id.value value
    
    override this.Parse(value: obj) =
        User_id (value :?> int64) 

type Group_id_mapper() =
    inherit SqlMapper.TypeHandler<Group_id>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            value: Group_id
        )
        =
        parameter.Value <- Group_id.value value
    
    override this.Parse(value: obj) =
        Group_id (value :?> int64) 


type Button_id_mapper() =
    inherit SqlMapper.TypeHandler<Button_id>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            value: Button_id
        )
        =
        parameter.Value <- Button_id.value value
    
    override this.Parse(value: obj) =
         value :?> string |> Button_id


type Language_mapper() =
    inherit SqlMapper.TypeHandler<Language>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            value: Language
        )
        =
        parameter.Value <- Language.to_string value
    
    override this.Parse(value: obj) =
        value :?> string
        |>Language.from_string

module Telegram_database_type_mappers =
    
    let set_telegram_type_handlers () =
        SqlMapper.AddTypeHandler(Timestamp_mapper())
        SqlMapper.AddTypeHandler(Question_id_mapper())
        SqlMapper.AddTypeHandler(User_id_mapper())
        SqlMapper.AddTypeHandler(Group_id_mapper())
        SqlMapper.AddTypeHandler(Button_id_mapper())
        SqlMapper.AddTypeHandler(Language_mapper())
        
        
