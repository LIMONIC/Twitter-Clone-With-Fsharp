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
                    hostname = ""192.168.1.41""
                }
            }
        }"
        )

let server = System.create "TwitterClone" (configuration)
let url = "akka.tcp://TwitterClone@192.168.1.41:9001/user/"
let PushHandlerRef = server.ActorSelection(url + "PushHandler")


// database
let databaseFilename = "Twitter.sqlite"
let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
let connection = new SQLiteConnection(connectionString)
type API = 
    | Req of (string) // request
    | Res of (string) // response

type Msg = 
    | Register of (string*string*JsonValue)// userid, pass
    | Login of (string*string*string)// userid, pass
    | Logout of (string*string)// userid, pass
    | Tweet of (string*JsonValue)// userid, pass, tweet_content
    | ReTweet of (string*JsonValue)// userid, pass, tweet_id
    | Follow of (string*JsonValue)// userid, pass, user_id
    | UnFollow of (string*JsonValue)// userid, pass, user_id
    | Query of (string*JsonValue)
    | Push of (string*string) // userId, tweetId
    | Default of (string)

let debug = true 
let mutable authedUserMap = Map.empty

type Utils() =
// Stringified JSON handling
    member this.parseRes status msg content = 
        sprintf """{"status": "%s","msg":"%s","content":%s}""" status msg content

    /// Generate a json string containing all tweets by SQLiteDataReader
    member this.getTweetJsonStr (reader : SQLiteDataReader) = 
        let mutable content = "["
        if  reader.HasRows then
            while reader.Read() do
                let tweetInfo = 
                    sprintf """{"text": "%s", "tweetId":"%s", "userId":"%s", "timestamp":"%s"}"""
                        (reader.["content"].ToString())
                        (reader.["id"].ToString())
                        (reader.["publish_user_id"].ToString())
                        (System.Convert.ToDateTime(reader.["timestamp"]).ToString("s"))
                content <- content + tweetInfo + ", "
            content.Substring(0, content.Length - 2) + "]"
        else 
            content + "]"
    /// Generate SHA1 for ID
    member this.getSHA1Str input = 
        let removeChar (stripChars:string) (text:string) =
            text.Split(stripChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) |> String.Concat
        System.Convert.ToString(DateTime.Now) + input
        |> Encoding.ASCII.GetBytes
        |> (new SHA1Managed()).ComputeHash
        |> System.BitConverter.ToString
        |> removeChar "-"
    /// compare elements in two lists and return same elements as list
    member this.compare2Lists oper l1 l2 = 
        let mutable list = []  
        l1 
        |> List.map (fun x -> (x, List.tryFind (oper x) l2))
        |> List.iter (function (x, None) -> () | (x, Some y) -> list <- list@[y])
        list
    /// Get a list from query result containing followers
    member this.getUserList columnName (reader : SQLiteDataReader) =
        let mutable followersList = []
        if  reader.HasRows then
            while reader.Read() do
                let follower = reader.[$"{columnName}"].ToString()
                followersList <- followersList @ [follower]
        followersList
let Utils = Utils()


type DB() =
// DB initiate
    member this.initDb _ = 
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

// DB operations
    member this.dbQuery queryStr = 
        let selectCommand = new SQLiteCommand(queryStr, connection)
        try 
            printfn $"dbQuery {selectCommand.ExecuteScalar().ToString()}"
            selectCommand.ExecuteScalar().ToString()
        with | _ -> "error"

    member this.dbQueryMany queryStr =
        let selectCommand = new SQLiteCommand(queryStr, connection)
        let reader = selectCommand.ExecuteReader()
        reader
    
    member this.dbInsert queryStr =
        let insertCommand = new SQLiteCommand(queryStr, connection)
        insertCommand.ExecuteNonQuery()

// DB record validation 
    member this.isUserExist userId =
        let res = this.dbQuery $"select id from User where id = '{userId}'"
        if debug then printfn $"-[DEBUG][isUserExist] res = {res}"
        not (res = "error")
        
    member this.isTweetIdExist tweetId = 
        let res = this.dbQuery $"select id from Tweet where id = '{tweetId}'"
        if debug then printfn $"-[DEBUG][isValidTweetId] res = {res}"
        not (res = "error")

    member this.isFollowed userId userIdTofollow = 
        let res = this.dbQuery $"select id from UserRelation where follower_id = '{userId}' and user_id='{userIdTofollow}'"
        if debug then printfn $"-[DEBUG][isFollowed] res = {res}"
        not (res = "error")

// DB queries
    /// Return a json string containing all tweets subscribed by a certain user
    member this.getSubscribedTweets userId =
        $"select t.content, t.id, t.publish_user_id, t.timestamp from UserRelation ur, Tweet t where ur.user_id=t.publish_user_id AND ur.follower_id='{userId}' ORDER BY timestamp ASC"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    /// Return the last 20 tweets
    member this.getLast20Tweets _ =
        "select t.content, t.id, t.publish_user_id, t.timestamp from Tweet t ORDER BY timestamp ASC limit 20"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    member this.getTweetsRelatedToTag tagName = 
        $"select t.content, t.id, t.publish_user_id, t.timestamp from HashTag ht, Tweet t where ht.tweet_id=t.id AND ht.tag_name='{tagName}' ORDER BY timestamp ASC"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    member this.getTweetsMentionById userId = 
        $"select t.content, t.id, t.publish_user_id, t.timestamp from Mention m, Tweet t where m.user_id='{userId}' AND m.tweet_id=t.id GROUP BY t.id ORDER BY timestamp ASC"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    member this.getTweetsMentionByName userName = 
        $"select t.content, t.id, t.publish_user_id, t.timestamp from User u, Mention m, Tweet t where u.nick_name = '{userName}' AND u.id=m.user_id AND m.tweet_id=t.id GROUP BY t.id ORDER BY timestamp ASC"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    member this.getTweetsById tweetId = 
        $"select t.content, t.id, t.publish_user_id, t.timestamp from Tweet t where id='{tweetId}'"
        |> this.dbQueryMany
        |> Utils.getTweetJsonStr

    member this.getFollowers userId = 
        $"SELECT follower_id FROM UserRelation WHERE user_id='{userId}'"
        |> this.dbQueryMany
        |> Utils.getUserList "follower_id"

    member this.getMentionedUsers tweetId = 
        $"select DISTINCT user_id from Mention where tweet_id='{tweetId}'"
        |> this.dbQueryMany
        |> Utils.getUserList "user_id"
let DB = DB()

type HandlerImpl() =
    member this.isUserLoggedIn userId = 
        authedUserMap.ContainsKey(userId)

// Register 
    member this.registerImpl (userId, password, nickName, email, (msg: byref<_>)) =
        let mutable flag = false
        if (nickName = "" || email = "") then 
            msg <- "Insufficient user information."
        else if DB.isUserExist userId then
            msg <- $"UserId {userId} has already been used. Please choose a new one."
        else
            let res = DB.dbInsert $"insert into User(id, email, nick_name, password) values ('{userId}', '{email}', '{nickName}', '{password}')"
            if res <> 1 then 
                msg <- "Registration Failed. Please try again."
            else
                msg <- $"Registration Success. Your user id is {userId}."
                flag <- true
        flag

// Login
    member this.loginImpl (userId, password, (msg: byref<_>)) =
        let mutable flag = false
        // check if already loged in
        if authedUserMap.ContainsKey(userId) then
            msg <- $"User {userId} has already logged in."
            flag <- true
        // find if user exist
        if not flag && not (DB.isUserExist userId) then 
                msg <- "User not existed. Please check the user information"
        // query user's password, check if password matches
        if not flag then 
            let pwRef = DB.dbQuery $"select password from User where id = '{userId}'"
            if password <> pwRef then
                msg <- "Login Failed. Please try again."
            else
                msg <- $"Login Success. You have logged in as {userId}"
                flag <- true
        flag

// Logout
    member this.logoutImpl (userId, password, (msg: byref<_>)) =
        let mutable flag = false
        // check if already loged in
        if not (authedUserMap.ContainsKey(userId)) then
            msg <- $"User {userId} has not logged in."
        // find if user exist
        if not flag && not (DB.isUserExist userId) then 
                msg <- "User not existed. Please check the user information"
        // query user's password, check if password matches
        if not flag then 
            let pwRef = DB.dbQuery $"select password from User where id = '{userId}'"
            if password <> pwRef then
                msg <- "Logout Failed. Please try again."
            else
                msg <- $"User {userId} logout Successed."
                authedUserMap <- authedUserMap.Remove(userId)
                flag <- true
        flag

// Tweet
    member this.processHashtag (tweetId, (hashtag: array<JsonValue>), (msg: byref<_>), (isSuccess: byref<_>)) = 
        if hashtag.Length > 0 then
            for tag in hashtag do 
                    let tagId = Utils.getSHA1Str (tag.AsString())
                    let dbRes = DB.dbInsert $"insert into HashTag(id, tag_name, tweet_id) values ('{tagId}', '{tag.AsString()}', '{tweetId}')"
                    if dbRes <> 1 then
                        msg <- "HashTag are not added properly."
                        isSuccess <- false

    member this.processMention (tweetId, (mention: array<JsonValue>), (msg: byref<_>), (isSuccess: byref<_>)) =
        if mention.Length > 0 then
            for user in mention do 
                    let mentionId = Utils.getSHA1Str (user.AsString())
                    let dbRes = DB.dbInsert $"insert into Mention(id, user_id, tweet_id) values ('{mentionId}', '{user.AsString()}', '{tweetId}')"
                    if dbRes <> 1 then 
                        msg <- "Mention are not added properly."
                        isSuccess <- false

    member this.tweetAndRetweetImpl ((tweetId: byref<_>), userId, param, (hashtag: array<JsonValue>), (mention: array<JsonValue>), (msg: byref<_>), oper) = 
        let mutable isSuccess = true
        let mutable text = ""
        // user must already logged in
        if not (this.isUserLoggedIn userId) then
            msg <- "User is not logged in, please log in again."
            isSuccess <- false
        // tweet mush has content
        else if (param = "") then
            if oper = "tweet" then msg <- "Empty tweet. Please add content."
            if oper = "retweet" then msg <- "Please specify which tweet you want to retweet."
            isSuccess <- false
        else 
            tweetId <- (Utils.getSHA1Str "")
            if oper = "tweet" then text <- param
            // Get original content for retweet
            if oper = "retweet" then text <- DB.dbQuery $"select content from Tweet where id = '{param}'"
            let res = DB.dbInsert $"insert into Tweet(id, content, publish_user_id, timestamp) values ('{tweetId}', '{text}', '{userId}', '{DateTime.Now}')"
            if res <> 1 then 
                if oper = "tweet" then msg <- "Tweet send failed. Please try again."
                if oper = "retweet" then msg <- "Retweet send failed. Please try again."
                isSuccess <- false
            else
                // process tags
                this.processHashtag (tweetId, hashtag, &msg, &isSuccess)
                // process mention
                this.processMention (tweetId, mention, &msg, &isSuccess)
        isSuccess

// Follow
    member this.isValidForFollowOrUnfollow (userId, targetUserId, (msg: byref<_>)) = 
        let mutable flag = false
        // user must already logged in
        if not (this.isUserLoggedIn userId) then
            msg <- "User is not logged in, please log in again."
        // target must not be null
        else if (targetUserId = "") then
            msg <- "Please specify which user you want to follow or unfollow."
        // target user must exist
        else if not (DB.isUserExist targetUserId) then
            msg <- $"User {targetUserId} not exist. Please check the user information"
        else
            flag <- true
        flag
    
    member this.followImpl (userId, userIdTofollow, (msg: byref<_>)) =
        let mutable flag = false
        if this.isValidForFollowOrUnfollow (userId, userIdTofollow, &msg) then
            if (DB.isFollowed userId userIdTofollow) then 
                msg <- $"User {userId} already followed user {userIdTofollow}."
            else
                // process follow
                let followId = Utils.getSHA1Str ""
                // check if already follewed
                let res = DB.dbInsert $"insert into UserRelation(id, user_id, follower_id) values ('{followId}', '{userIdTofollow}', '{userId}')"
                if res <> 1 then 
                    msg <- $"Follow user {userIdTofollow} failed. Please try again."
                else
                    msg <- $"{userId} successfully followed {userIdTofollow}."
                    flag <- true
        flag

    member this.unfollowImpl (userId, userIdToUnfollow, (msg: byref<_>)) =
        let mutable flag = false
        if this.isValidForFollowOrUnfollow (userId, userIdToUnfollow, &msg) then
            if not (DB.isFollowed userId userIdToUnfollow) then 
                msg <- $"User {userId} is not following user {userIdToUnfollow}."
                // query user relation
            else
                let userRelationId = DB.dbQuery $"select id from UserRelation where follower_id = '{userId}' AND user_id = '{userIdToUnfollow}'"
                if userRelationId = "error" then
                     msg <- $"You are not following user {userIdToUnfollow}."
                else
                // process unfollow
                    let res = DB.dbInsert $"delete from UserRelation where id='{userRelationId}'"
                    if res <> 1 then 
                        msg <- $"Unfollow user {userIdToUnfollow} failed. Please try again."
                    else
                        msg <- $"{userId} successfully unfollowed {userIdToUnfollow}."
                        flag <- true
        flag

    member this.queryImpl (userId, operation, tagId, mention, (msg: byref<_>), (status: byref<_>), (resJsonStr: byref<_>)) =
        // user must already logged in
        if not (this.isUserLoggedIn userId) then
            msg <- "User is not logged in, please log in again."
            resJsonStr <- Utils.parseRes status msg """[]"""
        else if (operation = "") then
            msg <- "Please specify query operation."
            resJsonStr <- Utils.parseRes status msg """[]"""
        else
            match operation with
            | "subscribe" -> 
                status <- "success"
                msg <- $"user {userId}'s subscribed tweets:"
                resJsonStr <- Utils.parseRes status msg (DB.getSubscribedTweets userId)
            | "all" ->
                status <- "success"
                msg <- "The latest 20 tweets:"
                resJsonStr <- Utils.parseRes status msg (DB.getLast20Tweets userId)
            | "tag" ->
                if (tagId = "") then
                    msg <- "Please specify which tagId you want to query."
                    resJsonStr <- Utils.parseRes status msg """[]"""
                else
                    status <- "success"
                    msg <- $"Tweets related to tag {tagId}:"
                    resJsonStr <- Utils.parseRes status msg (DB.getTweetsRelatedToTag tagId)
            | "mention" -> 
                if (mention = "") then
                    msg <- "Please specify which user you want to query."
                    resJsonStr <- Utils.parseRes status msg """[]"""
                else
                    let mutable tweets = DB.getTweetsMentionById mention
                    if tweets = "[]" then tweets <- DB.getTweetsMentionByName mention
                    status <- "success"
                    msg <- $"Tweets related to user {mention}:"
                    resJsonStr <- Utils.parseRes status msg tweets
            | _ -> 
                printfn $"[ERROR]Query operation not correct! operation: {operation}"
                msg <- $"Query operation not correct! operation: {operation}"
                resJsonStr <- Utils.parseRes status msg """[]"""
let Hi = HandlerImpl()

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
            if Hi.registerImpl (userId, password, nickName, email, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()


let LoginHandler (mailbox:Actor<_>) =
    // printfn "LoginHandler!!"
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]LoginHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Login(userId, password, remoteActorAddr) -> 
            if Hi.loginImpl(userId, password, &msg) then
                authedUserMap <- authedUserMap.Add(userId, remoteActorAddr)
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let LogoutHandler (mailbox:Actor<_>) =
    printfn "LogoutHandler!!"
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]LogoutHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Logout(userId, password) -> 
            if Hi.logoutImpl(userId, password, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let TweetHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]TweetHandler receive msg: {message}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        
        match message with
        | Tweet(userId, props) ->
            let content = try props?content.AsString() with |_ -> ""
            let hashtag = try props?hashtag.AsArray() with |_ -> [||]
            let mention = try props?mention.AsArray() with |_ -> [||]
            let mutable tweetId = ""
            // user must already logged in
            if Hi.tweetAndRetweetImpl (&tweetId, userId, content, hashtag, mention, &msg, "tweet") then 
                status <- "success"
                msg <- "Tweet sent."
                // push new tweet to followers
                // 1. get followers
                
                PushHandlerRef <! Push(userId, tweetId)
                
                // followers |> List.iteri(function i user -> )
                // let activeUsers = authedUserMap.
                // Utils.compare2Lists (=) followers 

                // 2. match followers with active users
                // 3. push the message to those users 

            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
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
        match message with
        | ReTweet(userId, props) -> 
            let reTweetId = try props?tweetId.AsString() with |_ -> ""
            let hashtag = try props?hashtag.AsArray() with |_ -> [||]
            let mention = try props?mention.AsArray() with |_ -> [||]
            let mutable tweetId = ""
            if Hi.tweetAndRetweetImpl (&tweetId, userId, reTweetId, hashtag, mention, &msg, "retweet")
                && (DB.isTweetIdExist reTweetId) then 
                status <- "success"
                msg <- "Retweet success."
                PushHandlerRef <! Push(userId, tweetId)
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
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
            let userIdTofollow = try props?userId.AsString() with |_ -> ""
            if Hi.followImpl (userId, userIdTofollow, &msg) then 
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
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
            let userIdToUnfollow = try props?userId.AsString() with |_ -> ""
            if Hi.unfollowImpl (userId, userIdToUnfollow, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            sender <! (resJsonStr)
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
            let tagId = try props?tagId.AsString() with |_ -> ""
            let mention = try props?mention.AsString() with |_ -> ""
            Hi.queryImpl (userId, operation, tagId, mention, &msg, &status, &resJsonStr)
            sender <! (resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let PushHandler (mailbox:Actor<_>) =
    let rec loop () = actor {
        let! message = mailbox.Receive()
        if debug then printfn $"[DEBUG]PushHandler receive msg: {message}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        match message with
        | Push(userId, tweetId) -> 
            let followers = DB.getFollowers userId
            if debug then printfn $"[Debug][PushHandler]:followers: {followers}"
            let mentionedUsers = DB.getMentionedUsers tweetId
            if debug then printfn $"[Debug][PushHandler]:mentionedUsers: {mentionedUsers}"
            // remove duplicated users
            let targetUsers = Set.union (Set.ofList followers) (Set.ofList mentionedUsers) |> Set.toList
            if debug then printfn $"[Debug][PushHandler]:targetUsers: {targetUsers}"
            // get tweet 
            let mutable tweet = "[]"
            tweet <- DB.getTweetsById tweetId
            // push tweet
            for x in targetUsers do
                if authedUserMap.ContainsKey(x) then
                    let remoteActorAddr = authedUserMap.[x]
                    let ipAddr = ((remoteActorAddr.Split '@').[1].Split ':').[0]
                    let addr = authedUserMap.[x] + "/user/client" + ipAddr
                    if debug then printfn $"[Debug][PushHandler]:Address: {addr}"
                    let remoteActorRef = server.ActorSelection(authedUserMap.[x] + "/user/client" + ipAddr)
                    msg <- "success"
                    status <- "New Tweet!"
                    resJsonStr <- Utils.parseRes msg status tweet
                    if debug then printfn $"[Debug] push to: {x} \tContent: {resJsonStr}"
                    remoteActorRef <! Res(resJsonStr)
        | _ -> ()
        return! loop()
    }
    loop()

let APIHandler (mailbox:Actor<_>) =
    printfn $"[INFO]: APIHandler on."
    let mutable handler = server.ActorSelection(url + "")
    let mutable msg = Default("")
    let rec loop () = actor {
        let! (message) = mailbox.Receive()
        let remoteActorAddr = mailbox.Sender().Path.Address.ToString()
        if debug then printfn $"[DEBUG]APIHandler remoteActorAddr: {remoteActorAddr}"
        let sender = mailbox.Sender()
        if debug then printfn $"[DEBUG]APIHandler receive msg: {message}"

        match message with
        | Req(info) -> 
            let infoJson = JsonValue.Parse(info)
            let operation = infoJson?api.AsString()
            let userId = infoJson?auth?id.AsString()
            let password = infoJson?auth?password.AsString()
            let props = infoJson?props
            if (operation = "" || userId = "" || password = "") then return! loop()
            match operation with
            | "Register" ->
                handler <- server.ActorSelection(url + "RegisterHandler")
                msg <- Register(userId, password, props)
            | "Login" ->
                handler <- server.ActorSelection(url + "LoginHandler")
                msg <- Login(userId, password, remoteActorAddr)
            | "Logout" -> 
                handler <- server.ActorSelection(url + "LogoutHandler")
                msg <- Logout(userId, password)
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
            | _ -> 
                printfn $"[ERROR]API not correct! API: {operation}"
                return! loop()
            let res = Async.RunSynchronously(handler <? msg, 100)
            sender <! Res(res)
        | _ -> ()
        return! loop()
    }
    loop()


DB.initDb()
spawn server "APIHandler" APIHandler
let actorPool = [("LoginHandler", LoginHandler); 
                 ("LogoutHandler", LogoutHandler); 
                 ("RegisterHandler", RegisterHandler); 
                 ("TweetHandler", TweetHandler); 
                 ("ReTweetHandler", ReTweetHandler); 
                 ("FollowHandler", FollowHandler); 
                 ("UnFollowHandler", UnFollowHandler); 
                 ("QueryHandler", QueryHandler);
                 ("PushHandler", PushHandler)]
for (name, actor) in actorPool do
    spawn server name actor |> ignore
connection.Open()

System.Console.Title <- "Server"
Console.ForegroundColor <- ConsoleColor.Green
printfn "Remote Actor %s listening..." server.Name
System.Console.ReadLine() |> ignore
0
// remoteSystem.Terminate().Wait()
// Console.ReadLine() |> ignore
