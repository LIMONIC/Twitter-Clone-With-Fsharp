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
                | _  -> printfn "Worker Received Wrong message"
                return! loop()
            }
            loop()
        )

let mutable uid = ""
let mutable pwd = ""

let testSuccess = "{\"status\": \"success\", \"msg\": \"test success.\",\"content\": []}"
let testError = "{\"status\": \"error\", \"msg\": \"test error.\",\"content\": []}"
let flag = false

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
   
    //printfn $"{info}"
    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
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

    //printfn $"{info}"
    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"

    //printfn $"{infoJson?status.AsString()}"
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

    //printfn $"{info}"

    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
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

    //printfn $"{info}"

    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let follow () = 
    printfn "\n[----------- Follow Page -----------]"
    printfn "\nEnter User ID to follow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"Follow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"
    
    //printfn $"{info}"
    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let unfollow () = 
    printfn "\n[----------- Unfollow Page -----------]"
    printfn "\nEnter User ID to unfollow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"UnFollow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"
    
    //printfn $"{info}"
    let response = (Async.RunSynchronously (userActor <? Req(info)))
    //let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let rec showTweets () = 
    printfn "\n[----------- Filter Tweets -----------]"
    printfn "1. subscribe\n2. tag\n3. mention\n4. all"
    printf "Enter your choice: "
    let operation = 
        match Int32.TryParse (Console.ReadLine()) with
        | true, 1 ->
            "subscribe"
        | true, 2 ->
            "tag"
        | true, 3 ->
            "mention"
        | true, 4 ->
            "all"
        | _ ->
            "error"
    if operation = "error" then 
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\t\tInvalid choice!"  
        printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        showTweets()
    else 
        let info = 
            if operation = "mention" then
                printfn "\n[----------- Query Mention -----------]"
                printf "\nEnter User ID or User Name: "
                let mentionid = Console.ReadLine()
                "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"operation\": \""+operation+"\",\"mention\": \""+mentionid+"\"}}"
            elif operation = "tag" then 
                printfn "\n[----------- Query Tag -----------]"
                printf "\nEnter tag ID: "
                let tagid = Console.ReadLine()
                "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"operation\": \""+operation+"\",\"tagId\": \""+tagid+"\"}}"
            else 
                "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"operation\": \""+operation+"\"}}"
    
        //printfn $"{info}"

        let testquery = "{\"status\": \"success\", \"msg\": \"The latest 20 tweets:\",\"content\": [{\"text\": \"Nice weather balabala!\", \"tweetId\": \"A813A096C85E37AA6890073A02576503ECA5015C\",\"userId\": \"Admin004\", \"timestamp\": \"2021-11-22T17:18:37\"},{\"text\": \"Suave is a simple web development F#\", \"tweetId\": \"B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD\",\"userId\": \"Admin006\", \"timestamp\": \"2021-11-22T17:14:10\"},{\"text\": \"its a query test\", \"tweetId\": \"CD5C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD\",\"userId\": \"Admin009\", \"timestamp\": \"2021-11-23T17:14:10\"}]}"
        let emptyquery = "{\"status\": \"success\", \"msg\": \"The latest 20 tweets:\",\"content\": []}"
        let response = (Async.RunSynchronously (userActor <? Req(info)))
        //let response = emptyquery
        let infoJson = FSharp.Data.JsonValue.Parse(response)
        let status = infoJson?status.AsString()
        let msg = infoJson?msg.AsString()
        printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn $"{msg}"
        printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        let tweets = infoJson?content.AsArray()
        for record in tweets do
            printfn $"Tweet ID: {record?tweetId.AsString()}"
            printfn $"User ID: {record?userId.AsString()}"
            printfn $"Time: {record?timestamp.AsString()}"
            printfn $"Content: \n{record?text.AsString()}"
            printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\nPress Any Key to the Main Page..."
        Console.ReadLine()

            


let rec mainMenu () =
    printfn "\n\n[----------- MAIN SCREEN -----------]"
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
        uid <- ""
        pwd <- ""
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