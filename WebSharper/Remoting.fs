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
open WebSharper.Json
open Utils
open Database

module Server =

    //printfn "%s" connection.FileName

    // Debug printout swithces
    let debug = true
    
    let testInfo = true

    type HandlerImpl() =
        member this.isUserLoggedIn userId =
            let ctx = Web.Remoting.GetContext()
            ctx.UserSession.IsAvailable

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
            let ctx = Web.Remoting.GetContext()
            if ctx.UserSession.IsAvailable then
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
            let ctx = Web.Remoting.GetContext()
            if not (ctx.UserSession.IsAvailable) then
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
    connection.Open()

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

    [<Rpc>]
    let DoLogout () = 
        if debug then printfn $"[DEBUG]LogoutHandler receive request"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        async {
            let ctx = Web.Remoting.GetContext()
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            let password = Array.get userinfo 1
            if Hi.logoutImpl(userId, password, &msg) then
                do! ctx.UserSession.Logout()
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            return resJsonStr
        }
    
    [<Rpc>]
    let DoLogin userId password = 
        if debug then printfn $"[DEBUG]LoginHandler receive userId: {userId}\npassword: {password}"
        // let mutable userIdSet = Set.empty
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let ctx = Web.Remoting.GetContext()
        async {
            if Hi.loginImpl(userId, password, &msg) then
//                authedUserMap <- authedUserMap.Add(userId, "") // TODO: login token
                do! ctx.UserSession.LoginUser(userId + "," + password, false)
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            return resJsonStr
        }
    [<Rpc>]
    let DoRegister (userId:string) (password:string) (props:string) =
        if debug then printfn $"[DEBUG]RegisterHandler receive userId: {userId}\npassword: {password}\nprops: {props}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let parsedProps = JsonValue.Parse(props)
        async {
            let nickName = parsedProps?nickName.AsString()
            let email = parsedProps?email.AsString()
            if Hi.registerImpl (userId, password, nickName, email, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            return resJsonStr
        }

    [<Rpc>]
    let DoTweet (props:string) =   
        if debug then printfn $"[DEBUG]TweetHandler receive props: {props}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let parsedProps = JsonValue.Parse(props)
        async {
            let ctx = Web.Remoting.GetContext()
            let! session = ctx.UserSession.GetLoggedInUser()
            printfn "%A" session
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            let password = Array.get userinfo 1
            let mutable tweetId = Utils.getSHA1Str ""
            let content = try parsedProps?content.AsString() with |_ -> ""
            let hashtag = try parsedProps?tag.AsArray() with |_ -> [||]
            let mention = try parsedProps?mention.AsArray() with |_ -> [||]
            if Hi.tweetAndRetweetImpl (&tweetId, userId, content, hashtag, mention, &msg, "tweet") then
                status <- "success"
                msg <- "Tweet sent."
                // push
            resJsonStr <- Utils.parseRes status msg (sprintf """[{"userId": "%s", "tweetId": "%s"}]""" userId tweetId)
            return resJsonStr
        }

    
    [<Rpc>]
    let DoReTweet (props:string) =   
        if debug then printfn $"[DEBUG]ReTweetHandler receive props: {props}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        let parsedProps = JsonValue.Parse(props)
        async {
            let ctx = Web.Remoting.GetContext()
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            let password = Array.get userinfo 1
            let mutable tweetId = Utils.getSHA1Str ""
            let retweetId = parsedProps?tweetId.AsString()
            let tag = parsedProps?tag.AsArray()
            let mention = parsedProps?mention.AsArray()
            if Hi.tweetAndRetweetImpl (&tweetId, userId, retweetId, tag, mention, &msg, "retweet") then
                status <- "success"
                msg <- "Retweet success."
            resJsonStr <- Utils.parseRes status msg (sprintf """[{"userId": "%s", "tweetId": "%s"}]""" userId tweetId)
            return resJsonStr
        }
        

    [<Rpc>]
    let DoFollow (followID:string) =   
        if debug then printfn $"[DEBUG]FollowHandler receive followID: {followID}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        async {
            let ctx = Web.Remoting.GetContext()
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            if Hi.followImpl (userId, followID, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            return resJsonStr
        }

    [<Rpc>]
    let DoUnfollow (unfollowID:string) =   
        if debug then printfn $"[DEBUG]UnfollowHandler receive unfollowID: {unfollowID}"
        let mutable status = "error"
        let mutable msg = "Internal error."
        let mutable resJsonStr = ""
        async {
            let ctx = Web.Remoting.GetContext()
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            if Hi.unfollowImpl (userId, unfollowID, &msg) then
                status <- "success"
            resJsonStr <- Utils.parseRes status msg """[]"""
            return resJsonStr
        }