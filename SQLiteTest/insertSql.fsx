#r "nuget: System.Data.SQLite"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#r "nuget: Akka.TestKit"

open Akka
open Akka.Actor
open Akka.FSharp
open System
open System.Collections.Generic
open System.Data.SQLite


let databaseFilename = "Twitter.sqlite"

let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
let connection = new SQLiteConnection(connectionString)
connection.Open()

// 1.c. Insert data
let insertSql = 
    "insert into User(id, email, nick_name, password) values ('0100', 'aaa@aa.com', 'admin', 'qwerty')"
let command = new SQLiteCommand(insertSql, connection)
command.ExecuteNonQuery() |> printfn "%A"


// 2. Query result
let selectSql = "select * from User"
let selectCommand = new SQLiteCommand(selectSql, connection)
let reader = selectCommand.ExecuteReader()
// printfn "%A" reader
while reader.Read() do
    printfn "%-7s %-19s %-19s %-19s" 
        (reader.["id"].ToString()) 
        (reader.["email"].ToString())
        (reader.["nick_name"].ToString())
        (reader.["password"].ToString())

connection.Close()


