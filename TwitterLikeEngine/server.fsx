#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#r "nuget: FSharp.Data"
#r "nuget: System.Data.SQLite"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Serialization.Hyperion"

open System
open System.Collections.Generic
open System.Data.SQLite
open System.Text
open System.Security.Cryptography
open FSharp.Data
open FSharp.Data.JsonExtensions
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open Akka.TestKit
open Akka.Remote
open Akka.Serialization
open System.Diagnostics
open System.IO

let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            actor {
                serializers {
                    hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                }
                serialization-bindings {
                    ""System.Object"" = hyperion
                }
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            }
            remote {
                helios.tcp {
                    transport-protocol = tcp
                    port = 9001
                    hostname = ""10.136.105.35""
                }
            }
        }"
        )

let server = System.create "TwitterClone" (configuration)
let url = "akka.tcp://TwitterClone@localhost:9001/user/"

// database
let databaseFilename = "Twitter.sqlite"
let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
let connection = new SQLiteConnection(connectionString)
type API = 
    // | Register of (string)// userid, pass
    // | Login of (string)// userid, pass
    // | Tweet of (string)// userid, pass, tweet_content
    // | ReTweet of (string)// userid, pass, tweet_id
    // | Follow of (string)// userid, pass, user_id
    // | UnFollow of (string)// userid, pass, user_id
    | Req of (string) // request
    | Res of (string) // response

type Msg = 
    | Register of (string*string*JsonValue)// userid, pass
    | Login of (string*string)// userid, pass
    | Tweet of (string*JsonValue)// userid, pass, tweet_content
    | ReTweet of (string*JsonValue)// userid, pass, tweet_id
    | Follow of (string*JsonValue)// userid, pass, user_id
    | UnFollow of (string*JsonValue)// userid, pass, user_id
    | Query of (string*JsonValue)
    | Default of (string)

let debug = true 
let mutable authedUserSet = Set.empty
// Res:
// {
//     "status":"",
//     "msg": "",
//     "content":[
//         {
//             "text":"",
//             "tweetId":"",
//             "userId":"",
//             "timestamp":""
//         },
//         {
//             "text":"",
//             "tweetId":"",
//             "userId":"",
//             "timestamp":""
//         }
//     ]
// }

// Req:
// {
//     "api":"",
//     "auth":{
//         "id":"t10000",
//         "password":1985
//     },
//     "props":{
//         "nickName":"test001",
//         "email":"test001@test.com"
//     }
// }

let dbQuery queryStr =
    let selectCommand = new SQLiteCommand(queryStr, connection)
    try 
        selectCommand.ExecuteScalar().ToString()
    with 
        | _ -> "error"

let dbQueryMany queryStr =
    let selectCommand = new SQLiteCommand(queryStr, connection)
    let reader = selectCommand.ExecuteReader()
    reader

let dbInsert queryStr =
    let insertCommand = new SQLiteCommand(queryStr, connection)
    insertCommand.ExecuteNonQuery()

let parseRes status msg content = 
    sprintf """{"status": "%s","msg":"%s","content":%s}""" status msg content

/// Generate a json string containing all tweets by SQLiteDataReader
let getTweetJsonStr (reader : SQLiteDataReader) = 
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
    else 
        content + "]"

/// Return a json string containing all tweets subscribed by a certain user
let getSubscribedTweets userId =
    $"select t.content, t.id, t.publish_user_id, t.timestamp from UserRelation ur, Tweet t where ur.user_id=t.publish_user_id AND ur.follower_id='{userId}' ORDER BY timestamp desc"
    |> dbQueryMany
    |> getTweetJsonStr

/// Return the last 20 tweets
let getLast20Tweets _ =
    "select t.content, t.id, t.publish_user_id, t.timestamp from Tweet t ORDER BY timestamp desc limit 20"
    |> dbQueryMany
    |> getTweetJsonStr

let getTweetsRelatedToTag tagName = 
    $"select t.content, t.id, t.publish_user_id, t.timestamp from HashTag ht, Tweet t where ht.tweet_id=t.id AND ht.tag_name='{tagName}' ORDER BY timestamp desc"
    |> dbQueryMany
    |> getTweetJsonStr

let getTweetsMentionById userId = 
    $"select t.content, t.id, t.publish_user_id, t.timestamp from Mention m, Tweet t where m.user_id='{userId}' AND m.tweet_id=t.id GROUP BY t.id ORDER BY timestamp desc"
    |> dbQueryMany
    |> getTweetJsonStr

let getTweetsMentionByName userName = 
    $"select t.content, t.id, t.publish_user_id, t.timestamp from User u, Mention m, Tweet t where u.nick_name = '{userName}' AND u.id=m.user_id AND m.tweet_id=t.id GROUP BY t.id ORDER BY timestamp desc"
    |> dbQueryMany
    |> getTweetJsonStr

let isUserExist userId =
    connection.Open()
    let isUserExist = dbQuery $"select id from User where id = '{userId}'"
    not (isUserExist = "error")

let isUserLoggedIn userId = 
    authedUserSet.Contains(userId)
/// Generate SHA1
let getSHA1Str input = 
    let removeChar (stripChars:string) (text:string) =
        text.Split(stripChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) |> String.Concat
    System.Convert.ToString(DateTime.Now) + input
    |> Encoding.ASCII.GetBytes
    |> (new SHA1Managed()).ComputeHash
    |> System.BitConverter.ToString
    |> removeChar "-"


let RegisterHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]RegisterHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Register(userId, password, props) -> 
            let nickName = props?nickName.AsString()
            let email = props?email.AsString()
            // input check
            if (nickName = "" || email = "") then 
                msg <- "Insufficient user information."
                resJsonStr <- parseRes status msg """[]"""
                sender <! Res(resJsonStr)
                return! loop()
            // find if user exist
            if isUserExist userId then 
                msg <- "UserId has already been used. Please choose a new one."
            else
                connection.Open()
                let res = dbInsert $"insert into User(id, email, nick_name, password) values ({userId}, {email}, {nickName}, {password})"
                if res = 1 then 
                    status <- "success"
                    msg <- "Registration Success."
                else
                    msg <- "Registration Failed. Please try again."
                connection.Close()
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let LoginHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]LoginHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let mutable pwRef = ""
        match message with
        | Login(userId, password) -> 
            // check if already loged in
            if authedUserSet.Contains(userId) then
                status <- "success"
                msg <- $"User {userId} has already logged in."
                resJsonStr <- parseRes status msg """[]"""
                sender <! Res(resJsonStr)
                return! loop()
            // find if user exist
            if isUserExist userId then 
                msg <- "User not existed. Please check the user information"
            else
                connection.Open()
                pwRef <- dbQuery $"select password from User where id = '{userId}'"
                if password = pwRef then 
                    status <- "success"
                    msg <- "Login Success."
                    authedUserSet <- authedUserSet.Add(userId)
                else
                    msg <- "Login Failed. Please try again."
                connection.Close()
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let TweetHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]TweetHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let mutable isSuccess = true
        match message with
        | Tweet(userId, props) ->
            let content = try props?content.AsString() with |_ -> ""
            let hashtag = try props?hashtag.AsArray() with |_ -> [||]
            let mention = try props?mention.AsArray() with |_ -> [||]
            // user must already logged in
            if isUserLoggedIn userId then 
            // tweet mush has content
                if (content = "") then
                    msg <- "Empty tweet. Please add content."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                // process tweet
                let tweetId = getSHA1Str ""
                let res = dbInsert "insert into Tweet(id, content, publish_user_id, timestamp) values ({tweetId}, {content}, {userId}, {DateTime.Now})"
                if res <> 1 then 
                    msg <- "Tweet send failed. Please try again."
                    isSuccess <- false
                // process tag
                if hashtag.Length > 0 then
                    [for tag in hashtag -> (
                            let tagId = getSHA1Str (tag.AsString())
                            let dbRes = dbInsert "insert into HashTag(id, tag_name, tweet_id) values ({tagId}, {hashtag}, {tweetId})"
                            if dbRes <> 1 then
                                msg <- "HashTag are not added properly."
                                isSuccess <- false
                    )] |> ignore                        
                // process mention
                if mention.Length > 0 then
                    [for user in mention -> 
                            let mentionId = getSHA1Str (user.AsString())
                            let dbRes = dbInsert "insert into Mention(id, user_id, tweet_id) values ({mentionId}, {user}, {tweetId})"
                            if dbRes <> 1 then 
                                msg <- "Mention are not added properly."
                                isSuccess <- false
                    ] |> ignore
                if isSuccess then 
                    status <- "success"
                    msg <- "Tweet sent."
            else 
                msg <- "User is not logged in, please log in again."
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let ReTweetHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]ReTweetHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let mutable isSuccess = true
        match message with
        | ReTweet(userId, props) -> 
            let reTweetId = try props?tweetId.AsString() with |_ -> ""
            let hashtag = try props?hashtag.AsArray() with |_ -> [||]
            let mention = try props?mention.AsArray() with |_ -> [||]
            // user must already logged in
            if isUserLoggedIn userId then 
            // tweet mush has content
                if (reTweetId = "") then
                    msg <- "Please specify which tweet you want to retweet."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                // get tweet
                let content = dbQuery $"select content from Tweet where id = '{reTweetId}'"
                // process tweet
                let tweetId = getSHA1Str ""
                let res = dbInsert "insert into Tweet(id, content, publish_user_id, timestamp) values ({tweetId}, {content}, {userId}, {DateTime.Now})"
                if res <> 1 then 
                    msg <- "Tweet send failed. Please try again."
                    isSuccess <- false
                // process tag
                if hashtag.Length > 0 then
                    [for tag in hashtag -> (
                            let tagId = getSHA1Str (tag.AsString())
                            let dbRes = dbInsert "insert into HashTag(id, tag_name, tweet_id) values ({tagId}, {hashtag}, {tweetId})"
                            if dbRes <> 1 then
                                msg <- "HashTag are not added properly."
                                isSuccess <- false
                    )] |> ignore                        
                // process mention
                if mention.Length > 0 then
                    [for user in mention -> 
                            let mentionId = getSHA1Str (user.AsString())
                            let dbRes = dbInsert "insert into Mention(id, user_id, tweet_id) values ({mentionId}, {user}, {tweetId})"
                            if dbRes <> 1 then 
                                msg <- "Mention are not added properly."
                                isSuccess <- false
                    ] |> ignore
                if isSuccess then 
                    status <- "success"
                    msg <- "Retweet success."
            else 
                msg <- "User is not logged in, please log in again."
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let FollowHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]FollowHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Follow(userId, props) -> 
            let followUserId = try props?userId.AsString() with |_ -> ""
            // user must already logged in
            if isUserLoggedIn userId then 
            // must has followUserId
                if (followUserId = "") then
                    msg <- "Please specify which user you want to follow."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                // process follow
                let followId = getSHA1Str ""
                let res = dbInsert "insert into UserRelation(id, user_id, follower_id) values ('{followId}', '{followUserId}', '{userId}')"
                if res <> 1 then 
                    msg <- $"Follow user {followUserId} failed. Please try again."
                else
                    status <- "success"
                    msg <- "Retweet success."
            else 
                msg <- "User is not logged in, please log in again."
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let UnFollowHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]UnFollowHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | UnFollow(userId, props) -> 
            let unfollowUserId = try props?userId.AsString() with |_ -> ""
            // user must already logged in
            if isUserLoggedIn userId then 
            // must has unfollowUserId
                if (unfollowUserId = "") then
                    msg <- "Please specify which user you want to unfollow."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                // query user relation
                let userRelationId = dbQuery $"select id from UserRelation where follower_id = '{userId}' AND user_id = '{unfollowUserId}'"
                if userRelationId = "error" then
                     msg <- $"You are not following user {unfollowUserId}."
                else
                // process unfollow
                    let res = dbInsert $"delete from UserRelation where id='{userRelationId}'"
                    if res <> 1 then 
                        msg <- $"Unfollow user {unfollowUserId} failed. Please try again."
                    else
                        status <- "success"
                        msg <- "Retweet success."
            else 
                msg <- "User is not logged in, please log in again."
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let QueryHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]QueryHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Query(userId, props) -> 
            let operation = try props?operation.AsString() with |_ -> ""
            // user must already logged in
            if isUserLoggedIn userId then 
            // must has operation
                if (operation = "") then
                    msg <- "Please specify which user you want to unfollow."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                match operation with
                | "subscribe " -> 
                    status <- "success"
                    msg <- $"user {userId}'s subscribed tweets:"
                    resJsonStr <- parseRes status msg (getSubscribedTweets userId)
                | "all" ->
                    status <- "success"
                    msg <- $"The latest 20 tweets:"
                    resJsonStr <- parseRes status msg (getLast20Tweets userId)
                | "tag" ->
                    let tagId = try props?tagId.AsString() with |_ -> ""
                    if (tagId = "") then
                        msg <- "Please specify which tagId you want to query."
                        resJsonStr <- parseRes status msg """[]"""
                        sender <! Res(resJsonStr)
                        return! loop()
                    status <- "success"
                    msg <- $"Tweets related to tag {tagId}:"
                    resJsonStr <- parseRes status msg (getTweetsRelatedToTag tagId)
                | "mention" -> 
                    let mention = try props?mention.AsString() with |_ -> ""
                    if (mention = "") then
                        msg <- "Please specify which user you want to query."
                        resJsonStr <- parseRes status msg """[]"""
                        sender <! Res(resJsonStr)
                        return! loop()
                    let mutable tweets = getTweetsMentionById mention
                    if tweets = "[]" then tweets <- getTweetsMentionByName mention
                    status <- "success"
                    msg <- $"Tweets related to user {mention}:"
                    resJsonStr <- parseRes status msg tweets
                | _ -> failwith "exception"
                sender <! Res(resJsonStr)
            else 
                msg <- "User is not logged in, please log in again."
            resJsonStr <- parseRes status msg """[]"""
            sender <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()


let APIHandler (mailbox:Actor<_>) =
    printfn $"[INFO]: APIHandler on."
    // TODO: Initiallize database
    

    let mutable handler = server.ActorSelection(url + "")
    let mutable msg = Default("")
    // todo: do auth
    
    let auth id pass = ()

    let rec loop () = actor {
        let! (message) = mailbox.Receive()
        let sender = mailbox.Sender()
        printfn "worker acotr receive msg: %A" message

        match message with
        | Req(info) -> 
            let infoJson = JsonValue.Parse(info)
            let operation = infoJson?api.AsString()
            let userId = infoJson?auth?id.AsString()
            let password = infoJson?auth?password.AsString()
            let props = infoJson?props
            if (operation = "" || userId = "" || password = "") then return! loop()
            printfn "operation: %A" operation
            match operation with
            | "Register" ->
                handler <- server.ActorSelection(url + "RegisterHandler")
                msg <- Register(userId, password, props)
            | "Login" ->
                handler <- server.ActorSelection(url + "LoginHandler")
                msg <- Login(userId, password)
            | "Tweet" ->
                handler <- server.ActorSelection(url + "TweetHandler")
                msg <- Tweet(userId, props)
            | "ReTweet" ->
                handler <- server.ActorSelection(url + "ReTweetHandler")
                msg <- ReTweet(userId, props)
            | "Follow" ->
                handler <- server.ActorSelection(url + "FollowHandler")
                msg <- Follow(userId, props)
            | "UnFollow" ->
                handler <- server.ActorSelection(url + "UnFollowHandler")
                msg <- UnFollow(userId, props)
            | "Query" ->
                handler <- server.ActorSelection(url + "QueryHandler")
                msg <- Query(userId, props)
            | _ -> return! loop()
        | _ -> ()
        let res = Async.RunSynchronously(handler <? msg, 100)
        sender <! res
        return! loop()
    }
    loop()

let initDb _ = 
    let databaseFilename = "Twitter.sqlite"
    let dbPath = @".\" + databaseFilename;
    // if DB file is not exist, initialize DB
    if not(File.Exists(dbPath)) then
        printfn $"[Info]DB doesn't exist {dbPath}. Initialize database..." 
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
        structureCommand.ExecuteNonQuery() |> ignore

initDb()
spawn server "APIHandler" APIHandler
System.Console.Title <- "Server"
Console.ForegroundColor <- ConsoleColor.Green
printfn "Remote Actor %s listening..." server.Name
System.Console.ReadLine() |> ignore
0
// remoteSystem.Terminate().Wait()
// Console.ReadLine() |> ignore