namespace WebSharper

open WebSharper
open WebSharper.JavaScript
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client

[<JavaScript>]
module TweetPushProcess =
    let ProcessBinding(ep: WebSocketEndpoint<Res, Push>) =
        let mutable wsServiceProvider: WebSocketServer<Res,Push> option = None
        
        let wsConnect =
            async {
                return! ConnectStateful ep <| fun ws -> async {
                    return 0, fun state msg -> async {
                        match msg with
                        | Open ->
                            Console.Log("WebSocket Connection Open")
                            return state
                        | Message data ->
                            match data with
                            | Res.Info (res) ->
                                Console.Log(res)
                                let resObj = JSON.Parse(res)
                                let div = JS.Document.GetElementById("tweetsDemoPanel")
                                let li = JS.Document.CreateElement("li")
                                li.AppendChild(JS.Document.CreateTextNode(resObj?text)) |> ignore
                                li.SetAttribute("class", "list-group-item")
                                div.AppendChild(li) |> ignore
                            return state + 1
                        | Close ->
                            Console.Log("WebSocket Connection Close")
                            return state
                        | Error ->
                            Console.Log("WebSocket Connection Error")
                            return state
                    }
                }
            }
        wsConnect.AsPromise().Then(fun x -> wsServiceProvider <- Some(x)) |> ignore