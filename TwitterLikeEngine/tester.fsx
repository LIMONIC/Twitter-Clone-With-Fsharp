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
                port = 9011
            }
        }" 
    )
let remoteSystem = System.create "TwitterClone" config

type API = 
    | Req of (string)
    | Res of (string)

type Info =
    | Init of (int)
    | SimuFollow of (string)
    | SimuTweet of (int)
    | SimuRetweet of (int)
    | StartTweet of (int)
    | Report of (int)

    

let getResponse res = 
    match res with
    | Res(resinfo) ->
        resinfo
    | _ -> 
        ""

let percentOf(number: int, percent: float) = 
    ((number |> float) * percent / 100.0) |> int

let args = fsi.CommandLineArgs |> Array.tail
let totalUsers = args.[0] |> int

let mutable celebrity : List<String> = List.Empty
let mutable influencer : List<String> = List.Empty
let mutable commonUser : List<String> = List.Empty
let mutable allUsers : List<String> = List.Empty
let celebrityCount = Math.Max(1, percentOf(totalUsers, 0.1))
let influencerCount = Math.Max(3, percentOf(totalUsers, 0.5))
let commonUserCount = totalUsers - (celebrityCount + influencerCount)
let celeFollowerRange = [(0.2 * (float) totalUsers) |> int; (0.3 * (float) totalUsers) |> int]
let influencerFollowerRange = [celeFollowerRange.[0] / 2; celeFollowerRange.[1] / 2]
let commonFollowerRnage = [1; celeFollowerRange.[1] / 3 |> int]
let celebrityTweetsCount = [200; 300]
let influencerTweetsCount = [80; 150]
let commonTweetsCount = [10; 30]
let offlineRange = [10; 15]
let sw = System.Diagnostics.Stopwatch()

let getWorkerById id =
    let actorPath = @"akka://TwitterClone/user/" + id
    select actorPath remoteSystem

let pickRandom (l: List<_>) =
    let r = System.Random()
    l.[r.Next(l.Length)]

let pickRandomUsers (l: List<_>) =
    let mutable list = List.Empty
    let r = System.Random()
    let nums = r.Next(10)
    for i in 0 .. nums do
        list <- list @ [(pickRandom allUsers)]
    list

let mutable celeFollwers = 0
let mutable influFollwers = 0
let mutable commonFollwers = 0
let mutable celeTweetsNum = 0
let mutable influTweetsNum = 0
let mutable commonTweetsNum = 0

let userActor uid = 
    spawn remoteSystem uid
        (fun mailbox ->
            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                let server = remoteSystem.ActorSelection (sprintf "akka.tcp://TwitterClone@192.168.171.128:9001/user/APIHandler")
                
                let login uid =
                    let info = "{\"api\": \"Login\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{}}"
                    server <! Req(info)
                  
                let logout uid =
                    let info = "{\"api\": \"Logout\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{}}"
                    server <! Req(info)
                  
                let followAPI uid followerid= 
                    let info = "{\"api\": \"Follow\",\"auth\": {\"id\":\""+followerid+"\",\"password\":\"111\"}, \"props\":{\"userId\": \""+uid+"\"}}"
                    server <! Req(info)
                
                let ranFollow uid (set : Set<String>) = 
                    let mutable followerid = pickRandom commonUser
                    while followerid = uid || set.Contains(followerid) do
                        followerid <- pickRandom commonUser
                    followAPI uid followerid
                    set.Add(followerid)
                    
                let ranFollowWithRange uid usertype = 
                    let r = Random()
                    let followerNum = 
                        if usertype = "celebrity" then  
                            let temp = r.Next(celeFollowerRange.[0], celeFollowerRange.[1])
                            celeFollwers <- celeFollwers + temp
                            temp
                        elif usertype = "influencer" then
                            let temp = r.Next(influencerFollowerRange.[0], influencerFollowerRange.[1])
                            influFollwers <- influFollwers + temp
                            temp
                        else 
                            let temp = r.Next(commonFollowerRnage.[0], commonFollowerRnage.[1])
                            commonFollwers <- commonFollwers + temp
                            temp
                    
                    let mutable set = Set.empty
                    for i in 1 .. followerNum do
                        set <- ranFollow uid set
                    printfn $"{usertype} {uid} has {followerNum} followers: {set}"
                    
                let tweetAPI uid content tagstring mentionstring =
                    let info = "{\"api\": \"Tweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"},  \"props\":{\"content\": \""+content+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"
                    server <! Req(info)
                
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
                
                let ranTweet () = 
                    let id = mailbox.Self.Path.Name
                    let content = ranStr(20)
                    let mention = ranMentions()
                    let tags = ranTags()
                    tweetAPI id content tags mention
                    //printfn $"{id} send tweet : {content}"

                let querySubTweetsAPI uid  = 
                    let info = "{\"api\": \"Query\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"}, \"props\":{\"operation\": \"subscribe\"}}"
                    getResponse(Async.RunSynchronously(server <? Req(info)))
                
                //let pickRandomTweet uid =
                //    let response = querySubTweetsAPI uid
                //    let infoJson = FSharp.Data.JsonValue.Parse(response)
                //    let tweets = infoJson?content.AsArray() |> Array.toList
                //    if not tweets.IsEmpty then 
                //        let ranTweet = pickRandom tweets
                //        ranTweet?tweetId.AsString()
                //    else ""
                
                let retweetAPI uid tweetId tagstring mentionstring =
                    let info = "{\"api\": \"ReTweet\",\"auth\": {\"id\":\""+uid+"\",\"password\":\"111\"},  \"props\":{\"tweetId\": \""+tweetId+"\",\"hashtag\": ["+tagstring+"], \"mention\":["+mentionstring+"]}}"
                    server <! Req(info)
                
                let ranRetweet (tweetId) = 
                    let id = mailbox.Self.Path.Name
                    let mention = ranMentions()
                    let tags = ranTags()
                    retweetAPI id tweetId tags mention
                    //printfn $"{id} retweet a tweet. Tweet ID: {tweetId}"

                match message with
                | Req (info) -> 
                    printfn $"{info}"
                | SimuFollow (utype) ->
                    let id = mailbox.Self.Path.Name
                    printfn $"{id}" 
                    ranFollowWithRange id utype
                | SimuTweet (tweetNum) ->
                    let id = mailbox.Self.Path.Name
                    login id 
                    for i in 1 .. tweetNum do
                        ranTweet()
                    logout id
                    printfn $"{id} finish tweet/retweet"
                    let boss = getWorkerById "Boss"
                    boss <! Report(1)
                | SimuRetweet (tweetNum) ->
                    let id = mailbox.Self.Path.Name
                    let response = querySubTweetsAPI id
                    let infoJson = FSharp.Data.JsonValue.Parse(response)
                    let tweets = infoJson?content.AsArray() |> Array.toList
                    login id
                    printfn $"{id} login"
                    for i in 1 .. tweetNum do
                        if not tweets.IsEmpty then
                            let ranTweet = pickRandom tweets
                            let tweetId = ranTweet?tweetId.AsString()
                            ranRetweet(tweetId)
                        else 
                            ranTweet()
                    logout id
                    printfn $"{id} finish tweet/retweet"
                    let boss = getWorkerById "Boss"
                    boss <! Report(1)
                | _ -> ()
                return! loop()
            }
            loop()
        )

let bossActor = 
    spawn remoteSystem ("Boss")
        (fun mailbox ->
            let mutable onlineReport = 0

            let rec loop() = actor {
                let! message = mailbox.Receive()
                let sender = mailbox.Sender()
                let server = remoteSystem.ActorSelection (sprintf "akka.tcp://TwitterClone@192.168.171.128:9001/user/APIHandler")
                
                
                let registerAPI uid pwd name email =
                    let info = "{\"api\": \"Register\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"nickName\": \""+name+"\",\"email\": \""+email+"\"}}"
                    server <! Req(info)


                let createUsers prefix num = 
                    for i in 0 .. num do
                        let uid = prefix + i.ToString()
                        let pwd = "111"
                        let name = prefix + i.ToString()
                        let email = prefix + i.ToString() + "@ufl.edu"
                        let info = "{\"api\": \"Register\",\"auth\": {\"id\":\""+uid+"\",\"password\":\""+pwd+"\"}, \"props\":{\"nickName\": \""+name+"\",\"email\": \""+email+"\"}}"
                        server <! Req(info)
                        userActor uid |> ignore
                        registerAPI uid pwd name email
                        if prefix = "celebrity" then 
                            celebrity <- celebrity @ [uid]
                        elif prefix = "influencer" then
                            influencer <- influencer @ [uid]
                        else
                            commonUser <- commonUser @ [uid]
                        allUsers <- allUsers @ [uid]

                let manageFollower uid utype =
                    let uact = getWorkerById uid
                    printfn $"{uact.PathString}"
                    uact <! SimuFollow(utype)

                let manageTweet uid utype =
                    let uact = getWorkerById uid
                    let r = Random()
                    let tweetnum = 
                        if utype = "celebrity" then  
                            let temp = r.Next(celebrityTweetsCount.[0], celebrityTweetsCount.[1])
                            celeTweetsNum <- celeTweetsNum + temp
                            temp
                        elif utype = "influencer" then
                            let temp = r.Next(influencerTweetsCount.[0], influencerTweetsCount.[1])
                            influTweetsNum <- influTweetsNum + temp
                            temp
                        else 
                            let temp = r.Next(commonTweetsCount.[0], commonTweetsCount.[1])
                            commonTweetsNum <- commonTweetsNum + temp
                            temp
                    if utype = "commonUser" then 
                        uact <! SimuRetweet(tweetnum)
                    else
                        uact <! SimuTweet(tweetnum)

                match message with
                | Init(info) -> 
                    createUsers "celebrity" celebrityCount
                    createUsers "influencer" influencerCount
                    createUsers "common" commonUserCount
                    printfn $"all users registered successfully, celebrity: {celebrityCount}, influencer: {influencerCount}, common: {commonUserCount}"
                    for cele in celebrity do
                        manageFollower cele "celebrity"
                    for influ in influencer do
                        manageFollower influ "influencer"
                    for common in commonUser do
                        manageFollower common "commonUser"

                    let boss = getWorkerById "Boss"
                    boss <! StartTweet(1)
                | StartTweet (info) ->
                    printfn "start Tweet Simulate"
                    sw.Start()
                    let set = Set.empty
                    let r = Random()
                    let offline = r.Next(offlineRange.[0], offlineRange.[1])
                    let online = totalUsers * (100 - offline) / 100 |> int;
                    onlineReport <- online
                    //onlineReport <- celebrity.Length + influencer.Length
                    for cele in celebrity do
                        manageTweet cele "celebrity"
                    for influ in influencer do
                        manageTweet influ "influencer"
                    for i in 1 .. (online - celebrity.Length - influencer.Length) do
                        let mutable commonid = pickRandom commonUser
                        while set.Contains(commonid) do
                            commonid <- pickRandom commonUser
                        manageTweet commonid "commonUser"
                | Report (info) ->
                    onlineReport <- onlineReport - 1
                    if onlineReport <= 0 then
                        sw.Stop()
                        let celeFollwersAvg = celeFollwers / celebrityCount
                        let influFollwersAvg = influFollwers / influencerCount
                        let commonFollwersAvg = commonFollwers / commonUserCount
                        let celeTweetsAvg = celeTweetsNum / celebrityCount
                        let influTweetsAvg = influTweetsNum / influencerCount
                        let commonTweetsAvg = commonTweetsNum / commonUserCount
                        printfn $"celebrityNum: {celebrityCount}   followers: {celeFollwers}   avg followers: {celeFollwersAvg}   tweets/retweets: {celeTweetsNum} avg tweets: {celeTweetsAvg}"  
                        printfn $"influencerNum: {influencerCount}   followers: {influFollwers}   avg followers: {influFollwersAvg}   tweets/retweets: {influTweetsNum} avg tweets: {influTweetsAvg}"  
                        printfn $"commonNum: {commonUserCount}   followers: {commonFollwers}   avg followers: {commonFollwersAvg}   tweets/retweets: {commonTweetsNum} avg tweets: {commonTweetsAvg}"  
                        printfn "Simulate finished in %A" sw.ElapsedMilliseconds
                        
                | _ -> ()
                return! loop()
            }
            loop()
        )


bossActor <! Init(0)
       

System.Console.ReadLine() |> ignore

