namespace WebSharper

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html
open WebSharper.AspNetCore.WebSocket
open WebSharper.JavaScript
open FSharp.Data
open FSharp.Data.JsonExtensions
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.Json

type EndPoint =
    | [<EndPoint "/">] Twitter
    // | [<EndPoint "/Twitter">] Twitter
    | [<EndPoint "/welcome">] Welcome
    | [<EndPoint "/register">] Register
    | [<EndPoint "/account">] Account
    // and userId = 001
    // and pass = "1234"

module Templating =
    
    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            // "Home" => EndPoint.Home
            "Twitter" => EndPoint.Twitter
            "Account" => EndPoint.Account
            // "Welcome" => EndPoint.Welcome
            // "Register" => EndPoint.Register
        ]

    

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
        
        

    let Twitter ctx action (body: Doc list) =
        Content.Page(
            Templates.TwitterTemplate()
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
        
        

    let Welcome ctx (body: Doc list) =
        Content.Page(
            Templates.LoginTemplate()
                .Title("Welcome")
                .Body(body)
                .Doc()
        )
        
        
        
        
    let Register ctx (body: Doc list)  =
        Content.Page(
            Templates.RegisterTemplate()
                .Title("Create your account")
                //.MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )

    let Account ctx action (body: Doc list)  =
        Content.Page(
            Templates.AccountTemplate()
                .MenuBar(MenuBar ctx action)
                //.MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
        
module Site =
    open WebSharper.UI.Html

    // let HomePage (ctx: Context<EndPoint>) =
    //     async {
    //         let! username = ctx.UserSession.GetLoggedInUser()
    //         let welcomeContent = 
    //             match username with
    //                 | None -> 
    //                     div [] [
    //                         h1 [] [text ("Welcome, stranger!")]
    //                         client <@Client.guest()@>
    //                     ]
    //                 | Some u ->   
    //                     let userinfo = u.Split(",")
    //                     div [] [
    //                         h1 [] [text ("Welcome back, " + userinfo.[0] + "!")]
    //                         client <@Client.LoggedInUser()@>
    //                     ]
    //         return! Templating.Main ctx EndPoint.Home "Home" [
    //             welcomeContent
    //             div [] [client <@ Client.Main() @>]
    //             // div [] [client <@ Client.Test() @>]
    //             div [] [text "!!!!"]
    //         ]
    //     }
        
        

    let Twitter (ctx: Context<EndPoint>) =
        let buildEndPoint(url: string): WebSocketEndpoint<Res, Push> =
                    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)
        let ep = buildEndPoint(ctx.RequestUri.ToString()) // WebSocket Endpoint
        async {
            let! username = ctx.UserSession.GetLoggedInUser()
            let welcomeContent = 
                match username with
                    | None -> 
                        div [] [
                            h1 [] [text ("Welcome, stranger!")]
                            client <@Client.guest()@>
                        ]
                    | Some u ->   
                        let userinfo = u.Split(",")
                        div [] [
                            h1 [] [text ("Welcome back, " + userinfo.[0] + "!")]
                            client <@Client.LoggedInUser()@>
                        ]
//            let tweetsDemoPanel =
//                div [attr.``class`` "jumbotron"; attr.``id`` "tweetsDemoPanel"] [
//                    h1 [] [text ("Result")]
//                    div [] [client <@ TweetPushProcess.ProcessBinding(ep) @>] 
//                ]
            return! Templating.Twitter ctx EndPoint.Twitter [
                welcomeContent
                div [] [client <@ Client.Twitter(ep) @>]
//                tweetsDemoPanel
            ]
        }

        
    let WelcomePage ctx =
        async {
            return! Templating.Welcome ctx [
                div [] [client <@ Client.Login()@>]
            ]
        }
        
        
    let RegisterPage ctx =
        async {
            return! Templating.Register ctx  [
                div [] [client <@ Client.Register() @>]
            ]
        }

    let AccountPage (ctx:Context<EndPoint>) =
        async {
            let! username = ctx.UserSession.GetLoggedInUser()
            
            match username with
                | None -> 
                    return! Templating.Welcome ctx [
                        div [] [client <@ Client.Login()@>]
                    ]
                | Some u ->   
                    return! Templating.Account ctx EndPoint.Account [
                        div [] [client <@ Client.Account() @>]
                    ]
        }
        

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            // | EndPoint.Home -> HomePage ctx
            | EndPoint.Twitter -> Twitter ctx
            | EndPoint.Welcome -> WelcomePage ctx
            | EndPoint.Register -> RegisterPage ctx
            | EndPoint.Account -> AccountPage ctx
        )
