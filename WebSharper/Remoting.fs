namespace WebSharper

open WebSharper
open System
open System.Data.SQLite
open System.IO
open System.Text
open System.Security.Cryptography
open System.Collections.Generic
open FSharp
open FSharp.Data
open FSharp.Data.JsonExtensions

module Server =
    // database
    let databaseFilename = "Twitter.sqlite"
    let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
    let connection = new SQLiteConnection(connectionString)

    // Debug printout swithces
    let debug = false
    let dbDebug = true
    let testInfo = true

    // Active user map <userId, userAddress>
    let mutable authedUserMap = Map.empty<string, string>

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
                if dbDebug then printfn $"[Info]DB doesn't exist {dbPath}. Initialize database..." 
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
                if dbDebug then printfn $"dbQuery {selectCommand.ExecuteScalar().ToString()}"
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
            if dbDebug then printfn $"-[DEBUG][isUserExist] res = {res}"
            not (res = "error")

        member this.isTweetIdExist tweetId = 
            let res = this.dbQuery $"select id from Tweet where id = '{tweetId}'"
            if dbDebug then printfn $"-[DEBUG][isValidTweetId] res = {res}"
            not (res = "error")

        member this.isFollowed userId userIdTofollow = 
            let res = this.dbQuery $"select id from UserRelation where follower_id = '{userId}' and user_id='{userIdTofollow}'"
            if dbDebug then printfn $"-[DEBUG][isFollowed] res = {res}"
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


    [<Rpc>]
    let DoTest str = printfn "!!!AAAAAAA!!! %s" str
    [<Rpc>]
    let DoDBInit () = 
        DB.initDb()
        if dbDebug then printfn "[Debug][DB] initDb() Done!" 
    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
     