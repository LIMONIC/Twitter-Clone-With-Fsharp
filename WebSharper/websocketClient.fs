module WebSharper.AspNetCore.Tests.WebSocketClient
open WebSharper
open WebSharper.JavaScript
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client
open WebSocketServer
open WebSharper.UI
open Newtonsoft.Json.Linq;
open System


module Server = WebSocketServer

// This us the client code that is viewed on the browser side.
[<JavaScript>]
let WebSocketTest (endpoint : WebSocketEndpoint<Server.S2CMessage, Server.C2SMessage>) =
    let mutable currentServer: WebSocketServer<WebSocketServer.S2CMessage,C2SMessage> option = None
    // This is used to hold the messages received from the server.
    let serverMessagesContainer = Elt.pre [] []
    let messagesHeader  =  Elt.div [] [
                                    Elt.h3[][text "Messages from server"]
                                ]

    // This method is used to write to server .
    let writen fmt =
        Printf.ksprintf (fun s ->
            JS.Document.CreateTextNode(s + "\n")
            |> serverMessagesContainer.Dom.AppendChild
            |> ignore
        ) fmt

    
    
    // This is used to connect with server using the websocket and .
    let server = 
        async{
            return! ConnectStateful endpoint <| fun server -> async {
                return 0, fun state msg-> async {
                    match msg with
                    // Got some data from server.
                    | Message data ->
                        match data with
                        | CommonResponse x -> 
                            // If type is tweet then add to tweets else add to server messages div.
                            if(x.Contains("By")&& x.Contains("Time") && x.Contains("Tweet")) then
                                let tweetDetails  =  x.JS.Split(",")
                                let by =  tweetDetails.[0].JS.Split("\"").[3]
                                let tweet=  tweetDetails.[1].JS.Split("\"").[3]
                                let time =  tweetDetails.[2].JS.Split("\"").[3]
                                addTweetInUi(by,tweet,time) 
                            else
                                writen "%s" x
                        | _ -> ignore()
                        return (state + 1)
                    | Close ->
                        writen "WebSocket connection closed."
                        return state
                    | Open ->
                        writen "WebSocket connection open."
                        return state
                    | Error ->
                        writen "WebSocket connection error!"
                        return state
                }
            }
        }
    // Initiate server.
    server.AsPromise().Then(fun x -> currentServer <- Some(x)) |> ignore
    // Some variables to populate the input fields.
    let registerUserName = Var.Create ""
    let subscribeUserName = Var.Create ""
    let unSubscribeUserName = Var.Create ""
    let tweetContent = Var.Create ""
    let hashTagQuery = Var.Create ""
    let mentionQuery = Var.Create ""

    // Based on the action performed by user send message to server.
    let enableOnClick action _  _ = 
        async {    
            if currentServer = None then
                JS.Alert("Retrying connection with the server. Please try again.")
                server.AsPromise().Then(fun x -> currentServer <- Some(x)) |> ignore
            match action with
            | "Register" ->
                if (registerUserName.Value.Length<6) then
                    JS.Alert("UserName must be atleast 6 characters.")
                else 
                    currentServer.Value.Post(Server.Register registerUserName.Value)
            | "Subscribe" -> 
                if (subscribeUserName.Value.Length<6) then
                    JS.Alert("UserName must be atleast 6 characters.")
                else 
                    currentServer.Value.Post(Server.Subscribe subscribeUserName.Value)
            | "Login" ->
                currentServer.Value.Post(Server.Login 1)
            | "Logout" ->
                currentServer.Value.Post(Server.Logout 1)
            | _ -> ignore()
            ignore()
        }
        |> Async.Start

    // Register new user form.
    let registerForm = 
        div [][
                Doc.Input [
                    attr.``class`` "form-control"]  registerUserName
                button [ 
                    attr.``class`` "btn btn-primary"
                    on.click (enableOnClick "Register") ] [ text "Register" ]
            ]

    // Login &  Logout buttons.
    let loginAndLogout = 
        div [attr.``class`` "user-login-logout"][
                button [attr.``class`` "btn btn-success" 
                        on.click (enableOnClick "Login") ] [ text "Login" ]
                button [attr.``class`` "btn btn-danger" 
                        on.click (enableOnClick "Logout") ] [ text "Logout" ]
            ]
    // Final page view.
    div []
        [ 
          loginAndLogout
          registerForm
          subscribeForm
          unsubscribeForm
          createTweetForm
          hashTagForm
          mentionForm
          messagesHeader
          serverMessagesContainer
          tweetContainer
        ]

open WebSharper.AspNetCore.WebSocket

let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<Server.S2CMessage, Server.C2SMessage> = 
    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)