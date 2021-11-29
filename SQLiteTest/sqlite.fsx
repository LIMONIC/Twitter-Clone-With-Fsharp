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

type TweetData = { 
    id:string; 
    content:string; 
    publishUserId:string;
    timestamp:DateTime }

// Sample Data
let tweets = [
    { id = "tw10000"; content = "content text 00000"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 33); };
    { id = "tw10001"; content = "content text 00001"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 21); };
    { id = "tw10002"; content = "content text 00002"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 21); };
    { id = "tw10003"; content = "content text 00003"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 21); };
    { id = "tw10004"; content = "content text 00004"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 03); };
    { id = "tw10005"; content = "content text 00005"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 44, 03); };
    { id = "tw10006"; content = "content text 00006"; publishUserId = "admin"; timestamp = new DateTime(2017, 07, 28, 10, 43, 31); } 
    ]

let databaseFilename = "Twitter.sqlite"

let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
// 0. Create Database
SQLiteConnection.CreateFile(databaseFilename)
// 1. Init Database
// 1.a. open connection
let connection = new SQLiteConnection(connectionString)
connection.Open()
// 1.b. Create tables
let structureSql =
    "create table Tweet (id TEXT, content TEXT, publish_user_id TEXT, timestamp TEXT);" +
    "create table User (id TEXT, email TEXT, nick_name TEXT, password TEXT);" +
    "create table UserRelation (id TEXT, user_id TEXT, follower_id TEXT);" +
    "create table HashTag (id TEXT, tag_name TEXT, tweet_id TEXT);" +
    "create table Mention (id TEXT, user_id TEXT, tweet_id TEXT)"
let structureCommand = new SQLiteCommand(structureSql, connection)
structureCommand.ExecuteNonQuery() 

// 1.c. Insert data
let insertSql = 
    "insert into Tweet(id, content, publish_user_id, timestamp) " + 
    "values (@id, @content, @publish_user_id, @timestamp)"
tweets
|> List.map(fun x ->
    use command = new SQLiteCommand(insertSql, connection)
    command.Parameters.AddWithValue("@id", x.id) |> ignore
    command.Parameters.AddWithValue("@content", x.content) |> ignore
    command.Parameters.AddWithValue("@publish_user_id", x.publishUserId) |> ignore
    command.Parameters.AddWithValue("@timestamp", x.timestamp) |> ignore
    command.ExecuteNonQuery())
|> List.sum
|> (fun recordsAdded -> printfn "Records added: %d" recordsAdded)


// 2. Query result
let selectSql = "select * from Tweet order by timestamp desc"
let selectCommand = new SQLiteCommand(selectSql, connection)
let reader = selectCommand.ExecuteReader()
printfn "%A" reader
while reader.Read() do
    printfn "%-7s %-19s %-19s %-19s" 
        (reader.["id"].ToString()) 
        (reader.["content"].ToString())
        (reader.["publish_user_id"].ToString())
        (System.Convert.ToDateTime(reader.["timestamp"]).ToString("s")) 

connection.Close()


