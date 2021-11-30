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

let args = fsi.CommandLineArgs |> Array.tail
let ip = args.[0]
let port = args.[1]

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
                hostname = ""%s""
                port = %s
            }
        }" ip port
    )
let remoteSystem = System.create "TwitterClone" config
//let server = remoteSystem.ActorSelection (sprintf "akka.tcp://TwitterClone@192.168.171.128:9001/user/APIHandler")

let mutable exit = false

type API = 
    | Req of (string)
    | Res of (string)

let mutable uid = ""
let mutable pwd = ""

let testSuccess = "{\"status\": \"success\", \"msg\": \"test success.\",\"content\": []}"
let testError = "{\"status\": \"error\", \"msg\": \"test error.\",\"content\": []}"
let flag = false
let mutable live = false

let getResponse res = 
    match res with
    | Res(resinfo) ->
        resinfo
    | _ -> 
        ""

let clientActor = 
    spawn remoteSystem ("client" + ip)
        (fun mailbox ->
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                let server = remoteSystem.ActorSelection (sprintf "akka.tcp://TwitterClone@192.168.1.41:9001/user/APIHandler")
                match message with
                | Req(info) -> 
                    //printf $"{info}"
                    let response = getResponse(Async.RunSynchronously(server <? Req(info)))
                    sender <! Res(response)
                | Res(info) ->
                    if live then 
                        let infoJson = FSharp.Data.JsonValue.Parse(info)
                        let msg = infoJson?msg.AsString()
                        printfn"\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
                        let tweets = infoJson?content.AsArray()
                        for record in tweets do
                            printfn $"Tweet ID: {record?tweetId.AsString()}"
                            printfn $"User ID: {record?userId.AsString()}"
                            printfn $"Time: {record?timestamp.AsString()}"
                            printfn $"Content: \n{record?text.AsString()}"
                            printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"  
                | _ -> ()
                return! loop()
            }
            loop()
        )

let registerUser() = 
    printf "Enter user ID:"
    uid <- Console.ReadLine()
    printf "Enter user name:"
    let name = Console.ReadLine()
    printf "Enter email address:"
    let email = Console.ReadLine()
    printf "Enter password:"
    pwd <- Console.ReadLine()
    let info = "{\"api\": \"Register\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"nickName\": \""+name+"\",\"email\": \""+email+"\"}}"
   
    let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
    //let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    if status = "error" then
        uid <- ""
        pwd <- ""
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let loginUser _ =
    printf "\nEnter User ID: "
    uid <- Console.ReadLine()
    printf "\nEnter Password: "
    pwd <- Console.ReadLine()

    let info = "{\"api\": \"Login\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{}}"
    
    let response = getResponse(Async.RunSynchronously(clientActor <? Req(info)))
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    if status = "error" then
        uid <- ""
        pwd <- ""
        false
    else
        true


let logoutUser _ =
    let info = "{\"api\": \"Logout\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{}}"
    
    let response = getResponse(Async.RunSynchronously(clientActor <? Req(info)))
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    if status <> "error" then
        uid <- ""
        pwd <- ""


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

    let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
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

    let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let follow () = 
    printfn "\n[----------- Follow Page -----------]"
    printf "\nEnter User ID to follow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"Follow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"

    let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
    //let response = if flag then testSuccess else testError

    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
    printfn $"{msg}"
    printfn"\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"


let unfollow () = 
    printfn "\n[----------- Unfollow Page -----------]"
    printfn "\nEnter User ID to unfollow: "
    let followid = Console.ReadLine()
    let info = "{\"api\": \"UnFollow\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"userId\": \""+followid+"\"}}"

    let response = getResponse(Async.RunSynchronously (clientActor <? Req(info)))
    //let response = if flag then testSuccess else testError
    
    let infoJson = FSharp.Data.JsonValue.Parse(response)
    let status = infoJson?status.AsString()
    let msg = "\t\t" + infoJson?msg.AsString()
    printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
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
        printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
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

        let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
        //let response = emptyquery
        let infoJson = FSharp.Data.JsonValue.Parse(response)
        let status = infoJson?status.AsString()
        let msg = infoJson?msg.AsString()
        printfn"\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn $"{msg}"
        printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        let tweets = infoJson?content.AsArray()
        for record in tweets do
            printfn $"Tweet ID: {record?tweetId.AsString()}"
            printfn $"User ID: {record?userId.AsString()}"
            printfn $"Time: {record?timestamp.AsString()}"
            printfn $"Content: \n{record?text.AsString()}"
            printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        printfn "\nPress Enter to the Main Page..."
        Console.ReadLine()

let tweetsPage() = 
    live <- true
    while live do
        let info = "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"operation\": \"all\"}}"
        let response = getResponse((Async.RunSynchronously (clientActor <? Req(info))))
        let infoJson = FSharp.Data.JsonValue.Parse(response)
        let msg = infoJson?msg.AsString()
        printfn"\n~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        let tweets = infoJson?content.AsArray()
        let mutable i = 0
        printfn $"number of all tweets: {Seq.length tweets}" 
        for record in tweets do
            i <- i + 1
            if i < 10 then 
                printfn $"Tweet ID: {record?tweetId.AsString()}"
                printfn $"User ID: {record?userId.AsString()}"
                printfn $"Time: {record?timestamp.AsString()}"
                printfn $"Content: \n{record?text.AsString()}"
                printfn"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
        let temp = Console.ReadLine()
        live <- false
        


let rec mainMenu () =
    printfn "\n\n[----------- MAIN SCREEN -----------]"
    printfn "1. Tweets Page\n2. Tweet\n3. ReTweet\n4. Follow\n5. Unfollow\n6. Query Tweets\n7. Refresh\n8. Logout\n9. Exit"
    printf "Enter your choice: "
    match Int32.TryParse (Console.ReadLine()) with
    | true, 1 -> 
        tweetsPage()
        mainMenu()
    | true, 2 -> 
        tweet()
        mainMenu()
    | true, 3 -> 
        retweet()
        mainMenu()
    | true, 4 ->
        follow()
        mainMenu()
    | true, 5 ->
        unfollow()
        mainMenu()
    | true, 6 ->
        showTweets() |> ignore
        mainMenu()
    | true, 7 ->
        mainMenu()
    | true, 8 ->
        logoutUser()
    | true, 9 ->
        printfn"\n\t\t~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~"
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
    if uid = "" && pwd = "" then menu()
    mainMenu()

System.Console.ReadLine() |> ignore
