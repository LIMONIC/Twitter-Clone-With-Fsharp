namespace WebSocketExample

open System.Collections.Generic
open WebSharper
open WebSharper.AspNetCore.WebSocket.Server

module WebSocketServiceProvider =
        
    let clients = HashSet<WebSocketClient<S2CMessage, C2SMessage>>()
    
    let Start(): StatefulAgent<S2CMessage, C2SMessage, int> =
        fun client -> async {
            if clients.Contains(client) = false then
                clients.Add(client) |> ignore
            return 0, fun state msg -> async {
                match msg with
                | Message data ->
                    match data with
                    | Speak content ->
                        let talkingInfo: TalkingInfo = Json.Deserialize content
                        let oneTalking = talkingInfo.username + ": " + talkingInfo.content
                        for cl in clients do
                            if cl <> client then
                                do! cl.PostAsync(Broadcast oneTalking)
                    return state + 1
                | Error error ->
                    printfn "%A" error
                    return state
                | Close ->
                    printfn "%s: Close Connection" (client.Context.RequestUri.AbsolutePath.ToString())
                    return state
            }
        }