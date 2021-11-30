#r "nuget: Akka.FSharp" 
#r "nuget: Akka.Remote"
open System
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
let configuration = 
    ConfigurationFactory.ParseString(
        @"akka {
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                deployment {
                    /remoteecho {
                        remote = ""akka.tcp://RemoteFSharp@127.0.0.1:9001""
                    }
                }
            }
            remote {
                helios.tcp {
                    port = 0
                    hostname = ""127.0.0.1""
                }
            }
        }")
type Information = 
    | TaskSize of (int64)
    | Input of (int64*int64*int64)
    | Output of (list<string * string>)
    | Register of (string)
    | Done of (string)

let system = ActorSystem.Create("StringDigger", configuration)
let echoClient = system.ActorSelection("akka.tcp://RemoteFSharp@127.0.0.1:9001/user/EchoServer")
// let echoClient = system.ActorSelection("akka.tcp://StringDigger@127.0.0.1:8778/user/receiver")
// echoClient <! "!!Msg to remote actor"
printfn "Reply from remote: %s" (string(Async.RunSynchronously (echoClient <? ("Msg to remote actor"), 1000)))