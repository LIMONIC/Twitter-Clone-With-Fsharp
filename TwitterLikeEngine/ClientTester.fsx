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
                hostname = ""10.136.105.35""
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

let coordinator (mailbox:Actor<_>) =
    let actcount = System.Environment.ProcessorCount |> int64
    printfn $"[INFO]: ProcessorCount: {actcount}"
    let api = "register"
    // let json = sprintf """{"api": "%s","auth":{"id":"t10000","password":1985},"props":{"nickName":"test001","email":"test001@test.com"}}""" api
    let json = sprintf """{"status": "%s","msg":"%s","content":%s}""" "200" "success" """[{"Jan": "Alexander"}]"""
    // printfn "%s" json
    // printfn "%A" (JsonValue.Parse(json))?auth
    let server = remoteSystem.ActorSelection ("akka.tcp://TwitterClone@10.136.105.35:9001/user/APIHandler")
    server <! Req(json)
    let rec loop () = actor {
        let! (message) = mailbox.Receive()
        printfn "worker acotr receive msg: %A" message
        let sender = mailbox.Sender()
        let server = remoteSystem.ActorSelection ("akka.tcp://TwitterClone@localhost:9001/user/APIHandler")
        server <! Res(json)
        //{ "id": "t10000", "password": 1985, "nickName": "test001", "email": "test001@test.com" }
        // match message with
        // | ServerInfo (info) -> 
        //     sender <! Res(actcount)
        // | _ -> ()
        return! loop()
    }
    loop()


spawn remoteSystem "coordinator" coordinator
System.Console.Title <- "Remote : " + System.Diagnostics.Process.GetCurrentProcess().Id.ToString()
Console.ForegroundColor <- ConsoleColor.Green
printfn "Remote Actor %s listening..." remoteSystem.Name
System.Console.ReadLine() |> ignore
0
// remoteSystem.Terminate().Wait()
// Console.ReadLine() |> ignore