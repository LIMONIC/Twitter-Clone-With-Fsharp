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
                hostname = ""192.168.171.128""
                port = 9100
            }
        }"
    )
let remoteSystem = System.create "TwitterClone" config

let mutable exit = false

type API = 
    | Req of (string)
    | Res of (string)

(*
operation: "Register","Login","Tweet","ReTweet","Follow","UnFollow","Query"
*)

let userActor = 
    spawn remoteSystem "User"
        (fun mailbox ->
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                let server = remoteSystem.ActorSelection ("akka.tcp://TwitterClone@10.136.105.35:9001/user/APIHandler")
                match message with
                | Req(info) -> 
                    let response = server <? Req(info)
                    sender <! response
                | Res(info) ->
                    printfn $"{info}"
                    sender <! info
                | _  -> printfn "Worker Received Wrong message"
                return! loop()
            }
            loop()
        )

let mutable uid = ""
let mutable pwd = ""

let testSuccess = "{\"status\": \"success\", \"msg\": \"test success.\",\"content\": []}"
let testError = "{\"status\": \"error\", \"msg\": \"test error.\",\"content\": []}"
let flag = true

let registerUser() = 
    printfn "Enter user ID:"
    uid <- Console.ReadLine()
    printfn "Enter user name:"
    let name = Console.ReadLine()
    printfn "Enter email address:"
    let email = Console.ReadLine()
    printfn "Enter password:"
    pwd <- Console.ReadLine()
    let info = "{\"api\": \"Register\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"nickName\": \""+name+"\",\"email\": \""+email+"\"}}"
   
    printfn $"{info}"
    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    if status = "error" then
        uid <- ""
        pwd <- ""
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let loginUser() =
    printfn "\nEnter User ID: "
    uid <- Console.ReadLine()
    printfn "\nEnter Password: "
    pwd <- Console.ReadLine()
    let info = "{\"api\": \"Login\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{}}"

    printfn $"{info}"
    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    if status = "error" then
        uid <- ""
        pwd <- ""
        false
    else
        true


let rec menu () =
    printfn "\n[----------- LOGIN | REGISTER -----------]"
    printfn "1. Login"
    printfn "2. Register"
    printf "Enter your choice: "
    match Int32.TryParse (Console.ReadLine()) with
    | true, 1 -> 
        let validLogin = loginUser()
        if not validLogin then
            menu()  
    | true, 2 -> 
        registerUser()
        menu()
    | _ ->
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\t\tInvalid choice!"  
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        menu()
  
let tweet () = 
    printfn "\n[----------- Tweet Page -----------]"
    printf "Enter your Tweet Content: "
    let content = Console.ReadLine()
    printf "Enter the tags (multiple tags seperated by ',' ):"
    let tags = Console.ReadLine().Split [|','|]
    let tagstring = "\"" + String.Join("\",\"", tags) + "\""

    printf "Enter the mentions (multiple mentions seperated by ',' ):"
    let mentions = Console.ReadLine().Split [|','|]
    let mentionstring = "\"" + String.Join("\",\"", mentions) + "\""
    
    let info = "{\"api\": \"Tweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"},  \"props\":{\"content\": \""+content+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"

    printfn $"{info}"

    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let retweet () = 
    printfn "\n[----------- ReTweet Page -----------]"
    printf "Enter the Tweet ID: "
    let tweetId = Console.ReadLine()
    printf "Enter the tags (multiple tags seperated by ',' ):"
    let tags = Console.ReadLine().Split [|','|]
    let tagstring = "\"" + String.Join("\",\" ", tags) + "\""
    printf "Enter the mentions: (multiple mentions seperated by ',' )"
    let mentions = Console.ReadLine().Split [|','|]
    let mentionstring = "\"" + String.Join("\",\" ", mentions) + "\""
    
    let info = "{\"api\": \"ReTweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"},  \"props\":{\"tweetId\": \""+tweetId+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"

    printfn $"{info}"

    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let follow () = 
    printfn "\n[----------- Follow Page -----------]"
    printfn "\nEnter User ID to follow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"Follow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"
    
    printfn $"{info}"
    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let unfollow () = 
    printfn "\n[----------- Unfollow Page -----------]"
    printfn "\nEnter User ID to unfollow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"UnFollow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"
    
    printfn $"{info}"
    //let response = (Async.RunSynchronously (userActor <? Req(info)))
    let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.ToString()
    let msg = "\t\t" + infoJson?msg.ToString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let showTweets () = 
    printfn "\n[----------- Tweet Page -----------]"



let rec mainMenu () =
    printfn "\n\n[----------- MAIN SCREEN -----------]"
    printfn "\n\nshow default tweets here!\n\n"
    printfn "1. Tweet\n2. ReTweet\n3. Follow\n4. Unfollow\n5. show Tweets\n6. Logout\n7. Exit"
    printf "Enter your choice: "
    match Int32.TryParse (Console.ReadLine()) with
    | true, 1 -> 
        tweet()
        mainMenu()
    | true, 2 -> 
        retweet()
        mainMenu()
    | true, 3 ->
        follow()
        mainMenu()
    | true, 4 ->
        unfollow()
        mainMenu()
    | true, 5 ->
        showTweets()
        mainMenu()
    | true, 6 ->
        uid <- ""
        pwd <- ""
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\t\tLogout Successfully!"  
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    | true, 7 ->
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\t\tGoodBye!"  
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        exit <- true
        ()
    | _ ->
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\t\tInvalid choice!"  
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        mainMenu()

while not exit do
    menu()
    mainMenu()

System.Console.ReadLine() |> ignore
