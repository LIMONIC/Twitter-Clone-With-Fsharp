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

module Utils =
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

