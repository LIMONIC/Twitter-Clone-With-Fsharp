#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#r "nuget: Akka.TestKit"
#r "nuget: FSharp.Data"
#r "nuget: Akka.Serialization.Hyperion"

open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open FSharp.Data
open FSharp.Data.JsonExtensions
open Akka.TestKit
open Akka.Remote
open Akka.Serialization
open System.Diagnostics

let config =
    ConfigurationFactory.ParseString(
        sprintf @"akka {
            actor {
                serializers {
                    hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                }
                serialization-bindings {
                    ""System.Object"" = hyperion
                } 
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            }
            remote.helios.tcp {
                hostname = ""192.168.171.128""
                port = 9010
            }
        }" 
    )
let remoteSystem = System.create "TwitterClone" config
let server = remoteSystem.ActorSelection (sprintf "akka.tcp://TwitterClone@192.168.171.128:9001/user/APIHandler")

let mutable users = List.Empty

type API = 
    | Req of (string)
    | Res of (string)

let getResponse res = 
    match res with
    | Res(resinfo) ->
        resinfo
    | _ -> 
        ""

let pickRandom (l: List<_>) =
    let r = System.Random()
    l.[r.Next(l.Length)]

let pickRandomUsers (l: List<_>) =
    let mutable list = List.Empty
    let r = System.Random()
    let nums = r.Next(10)
    for i in 0 .. nums do
        list <- list @ [(pickRandom users)]
    list

let registerAPI uid pwd name email =
    let info = "{\"api\": \"Register\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"nickName\": \""+name+"\",\"email\": \""+email+"\"}}"
    getResponse((Async.RunSynchronously (server <? Req(info))))

let createUsers prefix num = 
    for i in 0 .. num do
        let uid = prefix + i.ToString()
        let pwd = "111"
        let name = prefix + i.ToString()
        let email = prefix + i.ToString() + "@ufl.edu"
        registerAPI uid pwd name email |> ignore
        printfn $"{uid} register Successfully"
        users <- users @ [uid]

let loginAPI uid =
    let info = "{\"api\": \"Login\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{}}"
    getResponse(Async.RunSynchronously(server <? Req(info)))

let logoutAPI uid = 
    printf "logout"

let login uid =
    let response = loginAPI uid
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn $"{msg}"
   
let logout uid =
    let response = loginAPI uid
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn $"{msg}"
    

let tweetAPI uid content tagstring mentionstring =
    let info = "{\"api\": \"Tweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"},  \"props\":{\"content\": \""+content+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"
    getResponse((Async.RunSynchronously (server <? Req(info))))

let ranStr n = 
    let r = Random()
    let chars = Array.concat([[|'a' .. 'z'|];[|'A' .. 'Z'|];[|'0' .. '9'|]])
    let sz = Array.length chars in
    String(Array.init n (fun _ -> chars.[r.Next sz]))

let ranMentions _ = 
    "\"" + String.Join("\",\"", pickRandomUsers) + "\""

let ranTags _ =
    let mutable list = List.Empty
    let r = System.Random()
    let nums = r.Next(10)
    for i in 0 .. nums do
        list <- list @ [ranStr (r.Next(5))]
    "\"" + String.Join("\",\"", list) + "\""

let ranTweet uid = 
    let content = ranStr(20)
    let mention = ranMentions()
    let tags = ranTags()
    let response = tweetAPI uid content tags mention
    printfn $"{uid} send tweet : {content}"

let queryAllTweetsAPI uid  = 
    let info = "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{\"operation\": \"all\"}}"
    getResponse(Async.RunSynchronously(server <? Req(info)))

let pickRandomTweet uid =
    let response = queryAllTweetsAPI uid
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let tweets = infoJson?content.AsArray()
    let ranTweet = pickRandom tweets
    ranTweet?tweetId.AsString()

let retweetAPI uid tweetId tagstring mentionstring =
    let info = "{\"api\": \"ReTweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"},  \"props\":{\"tweetId\": \""+tweetId+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"
    getResponse((Async.RunSynchronously (server <? Req(info))))

let ranRetweet uid = 
    let tweetId = pickRandomTweet uid
    let mention = ranMentions()
    let tags = ranTags()
    let response = retweetAPI uid tweetId tags mention
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    printfn $"{uid} retweet a tweet. Tweet ID: {tweetId}"

let followAPI uid followid= 
    let info = "{\"api\": \"Follow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{\"userId\": \""+followid+"\"}}"
    getResponse((Async.RunSynchronously (server <? Req(info))))

let ranFollow uid = 
    let followid = pickRandom users
    let response = followAPI uid followid
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    printfn $"{uid} follow {followid}"


let args = fsi.CommandLineArgs |> Array.tail
let usernum = args.[0] |> int

createUsers "testUser" usernum
//a user process: login -> ranTweet -> ranRetweet -> ranFollow -> logout
let simulateUser _ = 
    let uid = pickRandom users
    //要实现异步每几秒执行一次
    login(uid)
    ranTweet(uid)
    ranRetweet(uid)
    ranFollow(uid)
    logout(uid)


