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
    | Req of (string)
    | Res of (string)

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

let isUserExist userId =
    connection.Open()
    let isUserExist = dbQuery $"select id from User where id = '{userId}'"
    not (isUserExist = "error")

let isUserLoggedIn userId = 
    authedUserSet.Contains(userId)
/// Generate SHA1
let getSHA1Str _ = 
    let removeChar (stripChars:string) (text:string) =
        text.Split(stripChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) |> String.Concat
    System.Convert.ToString(DateTime.Now)
    |> Encoding.ASCII.GetBytes
    |> (new SHA1Managed()).ComputeHash
    |> System.BitConverter.ToString
    |> removeChar "-"


let RegisterHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        printfn "worker acotr receive msg: %A" message
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
        if debug then printfn $"LoginHandler receive msg: {message}"
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
        printfn "worker acotr receive msg: %A" message
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Tweet(userId, props) ->
            let content = try props?content.AsString() with |_ -> ""
            let hashtag = try props?hashtag.AsString() with |_ -> ""
            let mention = try props?mention.AsString() with |_ -> ""
            // user must already logged in
            if isUserLoggedIn userId then 
            // tweet mush has content
                if (content = "") then
                    msg <- "Empty tweet. Please add content."
                    resJsonStr <- parseRes status msg """[]"""
                    sender <! Res(resJsonStr)
                    return! loop()
                // process tweet
                let res = dbInsert "insert into Tweet(id, content, publish_user_id, timestamp) values ({getSHA1Str()}, {content}, {userId}, {DateTime.Now})"
                if res = 1 then 
                    status <- "success"
                    msg <- "Tweet sent."
                else
                    msg <- "Tweet send failed. Please try again."
                // process tag
                if hashtag <> "" then
                // TODO
                    ""
                // process mention
                if mention <> "" then
                // TODO
                    ""
            else 

        | _ -> ()
        return! loop()
    }
    loop()

let ReTweetHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        printfn "worker acotr receive msg: %A" message
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | TweReTweet(userId, props) -> ()
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


spawn server "APIHandler" APIHandler
System.Console.Title <- "Server"
Console.ForegroundColor <- ConsoleColor.Green
printfn "Remote Actor %s listening..." server.Name
System.Console.ReadLine() |> ignore
0
// remoteSystem.Terminate().Wait()
// Console.ReadLine() |> ignore