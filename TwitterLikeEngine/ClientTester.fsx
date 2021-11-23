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
            remote.helios.tcp {
                hostname = ""192.168.1.41""
                port = 9100
            }
        }"
    )
let remoteSystem = System.create "TwitterClone" config

type API = 
    | Req of (string)
    | Res of (string)

// Req:
// {
//     "api":"register",
//     "auth":{
//         "id":"t10000",
//         "password":1985
//     },
//     "props":{
//         "nickName":"test001",
//         "email":"test001@test.com"
//     }
// }

// let coordinator (mailbox:Actor<_>) =
//     let actcount = System.Environment.ProcessorCount |> int64
//     printfn $"[INFO]: ProcessorCount: {actcount}"

    // let rec loop () = actor {
    //     let! (message) = mailbox.Receive()
    //     printfn "worker acotr receive msg: %A" message
    //     let sender = mailbox.Sender()
    //     let server = remoteSystem.ActorSelection ("akka.tcp://TwitterClone@localhost:9001/user/APIHandler")
    //     server <! Res(json)
    //     //{ "id": "t10000", "password": 1985, "nickName": "test001", "email": "test001@test.com" }
    //     // match message with
    //     // | ServerInfo (info) -> 
    //     //     sender <! Res(actcount)
    //     // | _ -> ()
    //     return! loop()
    // }
    // loop()


// spawn remoteSystem "coordinator" coordinator

let tester _ = 
    let api = "Query" // "Login" "Register" "Tweet" "ReTweet" "Follow" "UnFollow" "Query"
    let id = "Admin007"
    let password = "Admin007"
    let prop = """{}"""
    let registerProp = sprintf """{"email": "%s", "nickName": "%s"}"""
                            "test7@test.com"
                            "Wang"
    let loginProp = """{}"""

    let tweetProp1 = sprintf """{"content": "%s"}"""
                        "new tweet!"
    let tweetProp2 = sprintf """{"content": "%s", "hashtag": ["test1", "test2"]}"""
                        "Another tweet!"
    let tweetProp3 = sprintf """{"content": "%s", "hashtag": ["Suave", "test7"], "mention": ["Admin001", "Admin002"]}"""
                        "Suave is a simple web development F#"

    let retweetProp = sprintf """{"tweetId": "%s"}"""
                        "0F135E30DCF16D471B8219B55431B61F9321F884"
    let retweetProp1 = sprintf """{"tweetId": "%s"}, "hashtag": ["Suave", "test8"], "mention": ["Admin003", "Admin004"]"""
                        "0F135E30DCF16D471B8219B55431B61F9321F884"

    let followProp = sprintf """{"userId": "%s"}"""
                        "Admin005"
    let unfollowProp = sprintf """{"userId": "%s"}"""
                        "Admin001"

    let queryProp1 = sprintf """{"operation": "%s"}""" //subscribe || tag || mention || all
                        "subscribe"
    let queryProp2 = sprintf """{"operation": "%s", "tagId": "Suave"}""" //subscribe || tag || mention || all
                        "tag"
    let queryProp3 = sprintf """{"operation": "%s", "mention": "Bob"}""" //subscribe || tag || mention || all
                        "mention"
    let queryProp4 = sprintf """{"operation": "%s"}""" //subscribe || tag || mention || all
                        "all"
    let json = sprintf """{"api": "%s","auth":{"id":"%s","password":"%s"},"props":%s}"""
                    api id password queryProp4
    // let json = sprintf """{"status": "%s","msg":"%s","content":%s}""" "200" "success" """[{"Jan": "Alexander"}]"""
    // printfn "%s" json
    // printfn "%A" (JsonValue.Parse(json))?auth
    let server = remoteSystem.ActorSelection ("akka.tcp://TwitterClone@192.168.1.41:9001/user/APIHandler")
    let response = Async.RunSynchronously(server <? Req(json))

    match response with
    | Res(info) ->
        printfn "------------Request-------------"
        printfn "%A" (JsonValue.Parse(json))
        printfn "------------Response------------"
        printfn "%A"  (JsonValue.Parse(info))
        printfn "--------------------------------"
    | _ -> 
        printfn "[ERROR] response: \n{response}"

tester()
System.Console.Title <- "Remote : " + System.Diagnostics.Process.GetCurrentProcess().Id.ToString()
Console.ForegroundColor <- ConsoleColor.Green
printfn "Remote Actor %s listening..." remoteSystem.Name
System.Console.ReadLine() |> ignore
0
// remoteSystem.Terminate().Wait()
// Console.ReadLine() |> ignore