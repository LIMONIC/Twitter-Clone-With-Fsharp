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

let databaseFilename = "Twitter.sqlite"

let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  

let connection = new SQLiteConnection(connectionString)
connection.Open()
// 2. Query result
let selectSql = "select id from User where id = '0100'"
let selectCommand = new SQLiteCommand(selectSql, connection)
try 
    selectCommand.ExecuteScalar().ToString()
    |> printfn "%A"
with 
    | _ -> printfn "%A" ("error")

// printfn "%A" selectCommand.ToString


// printfn "%A" (reader.["id"].ToString())
// printfn "%A" reader.HasRows


// let mutable set = Set.empty
// while reader.Read() do
//     printfn "%A" (reader.["password"].ToString())
//     set <- set.Add(reader.["id"].ToString())
    // printfn "%-7s %-19s %-19s %-19s" 
    //     (reader.["id"].ToString()) 
    //     (reader.["content"].ToString())
    //     (reader.["publish_user_id"].ToString())
    //     (System.Convert.ToDateTime(reader.["timestamp"]).ToString("s")) 
// printfn "%A" (set.Contains("0003"))

let dbQueryMany queryStr =
    let selectCommand = new SQLiteCommand(queryStr, connection)
    let reader = selectCommand.ExecuteReader()
    reader

let getSubscribedTweet userId =
    let query = $"select t.content, t.id, t.publish_user_id, t.timestamp from UserRelation ur, Tweet t where ur.user_id=t.publish_user_id AND ur.follower_id='{userId}' ORDER BY timestamp desc"
    let reader = dbQueryMany query
    let mutable content = "["
    if  reader.HasRows then
        while reader.Read() do
            let tweetInfo = 
                sprintf """{"text": "%s", "tweetId":"%s", "userId":"%s", "timestamp":"%s"}"""
                    (reader.["content"].ToString())
                    (reader.["id"].ToString())
                    (reader.["id"].ToString())
                    (System.Convert.ToDateTime(reader.["timestamp"]).ToString("s"))
            content <- content + tweetInfo + ", "
    content.Substring(0, content.Length - 2) + "]"
let res = getSubscribedTweet "0102"
printfn "%A" res
connection.Close()


