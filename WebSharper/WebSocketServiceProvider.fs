namespace WebSharper

open System.Collections.Generic
open Microsoft.FSharp.Collections
open WebSharper
open WebSharper.AspNetCore.WebSocket.Server
open Utils
open Database

module WebSocketServiceProvider =
    let debug = true
    let mutable authedUserMap = Map.empty //    <WebSocketClient<Res, Push>, string>
    
    let Start(): StatefulAgent<Res, Push, int> =
        fun client -> async {
            // get user Id from WebSocketClient
            let! session = client.Context.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            if debug then printfn $"[Debug][client.Context]: {userinfo.[0]}"
            // Hashset "authedUserMap" contains all WebSocketClient that connected to the server
            if authedUserMap.ContainsKey(userinfo.[0]) = false then
                authedUserMap <- authedUserMap.Add(userinfo.[0], client)
            if debug then printfn $"[Debug][authedUserMap Size] {authedUserMap.Count}"
            return 0, fun state msg ->
                if debug then printfn $"state: {state}; msg: {msg}"
                async {
                match msg with
                | Message data ->
                    let mutable status = "error"
                    let mutable msg = "Internal error."
                    let mutable resJsonStr = ""
                    match data with
                    | Push.Info (userId, tweetId) ->
                        let followers = DB.getFollowers userId
                        if debug then printfn $"[Debug][PushHandler]:targetUsers: {followers}"
                        let mentionedUsers = DB.getMentionedUsers tweetId
                        // remove duplicated users
                        let targetUsers = Set.union (Set.ofList followers) (Set.ofList mentionedUsers) |> Set.toList
                        if debug then printfn $"[Debug][PushHandler]:targetUsers: {targetUsers}"
                        // get tweet 
                        let mutable tweet = "[]"
                        tweet <- DB.getTweetsById tweetId
                        for u in targetUsers do
                            if (not (u = null || u = "")) && authedUserMap.ContainsKey(u) then
                                let targetClient = authedUserMap.[u]
                                msg <- "success"
                                status <- "New Tweet!"
                                resJsonStr <- Utils.parseRes msg status tweet
                                if debug then printfn $"[Debug] push to: {u} \tContent: {resJsonStr}"
                                do! targetClient.PostAsync(Res.Info (resJsonStr))
                    return state + 1
                | Error error ->
                    printfn "%A" error
                    return state
                | Close ->
                    printfn "%s: Close Connection" (client.Context.RequestUri.AbsolutePath.ToString())
                    return state
            }
        }