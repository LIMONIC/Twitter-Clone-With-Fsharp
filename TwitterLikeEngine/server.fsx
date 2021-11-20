#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#r "nuget: FSharp.Data"
#r "nuget: Akka.TestKit"
#r "nuget: Akka.Serialization.Hyperion"

open System
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
                    hostname = ""localhost""
                }
            }
        }"
        )

let server = System.create "TwitterClone" (configuration)

type API = 
    | Register of (string)
    | Login of (string)
    | Tweet of (string)
    | ReTweet of (string)
    | Follow of (string)
    | UnFollow of (string)
    | Req of (string)
    | Res of (string)


let APIHandler (mailbox:Actor<_>) =
    printfn $"[INFO]: APIHandler on."
    // TODO: Initiallize database
    let rec loop () = actor {
        let! (message) = mailbox.Receive()
        // printfn "worker acotr receive msg: %A" message
        let sender = mailbox.Sender()
        match message with
        | Register (info) -> ()
        | Login (info) -> ()
        | Tweet (info) -> ()
        | ReTweet (info) -> ()
        | Follow (info) -> ()
        | UnFollow (info) -> ()
        | Req (info) -> ()
        | Res (info) -> ()
        | _ -> return! loop()
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