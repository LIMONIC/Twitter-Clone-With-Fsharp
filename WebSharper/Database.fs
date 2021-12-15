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

module Database =
    let dbDebug = true
    
    // database
    let databaseFilename = "Twitter.sqlite"
    let connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename  
    let connection = new SQLiteConnection(connectionString)
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
            
        member this.getFollows userId = 
            $"SELECT user_id FROM UserRelation WHERE follower_id='{userId}'"
            |> this.dbQueryMany
            |> Utils.getUserList "user_id"
            
        member this.getMentionedUsers tweetId = 
            $"select DISTINCT user_id from Mention where tweet_id='{tweetId}'"
            |> this.dbQueryMany
            |> Utils.getUserList "user_id"
    let DB = DB()