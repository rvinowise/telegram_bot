namespace rvinowise.telegram_defender

open System
open System.Data
open System.Data.SqlTypes
open Dapper
open rvinowise.telegram_defender


      
type Timestamp_mapper() =
    (* by default, Dapper transforms time to UTC when writing to the DB,
    but on some machines it doesn't transform it back when reading,
    it stays as UTC *)
    inherit SqlMapper.TypeHandler<DateTime>()
    override this.SetValue(
            parameter:IDbDataParameter ,
            datetime_value: DateTime
        )
        =
        let utl_value =
            if datetime_value.Kind = DateTimeKind.Utc then
                datetime_value
            else
                let utc_offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)
                datetime_value.Add(-utc_offset)  
                
        parameter.Value <- utl_value
    
    override this.Parse(value: obj) =
        let retrieved_datetime =
            value :?> DateTime
        if retrieved_datetime.Kind = DateTimeKind.Utc then
            let utc_offset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow)
            retrieved_datetime.Add(utc_offset)  
        else
            retrieved_datetime

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



module Telegram_database_type_mappers =
    
    let set_telegram_type_handlers () =
        //SqlMapper.AddTypeHandler(Timestamp_mapper()) //sometimes it's needed, sometimes not
        SqlMapper.AddTypeHandler(Question_id_mapper())
        SqlMapper.AddTypeHandler(User_id_mapper())
        SqlMapper.AddTypeHandler(Group_id_mapper())
        
        
