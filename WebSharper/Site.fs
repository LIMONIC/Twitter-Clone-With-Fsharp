namespace WebSharper

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.JavaScript
open FSharp.Data
open FSharp.Data.JsonExtensions

type EndPoint =
    | [<EndPoint "/">] Home
    | [<EndPoint "/about">] About
    | [<EndPoint "/Twitter">] Twitter
    | [<EndPoint "/welcome">] Welcome
    | [<EndPoint "/register">] Register
    // and userId = 001
    // and pass = "1234"

module Templating =
    open WebSharper.UI.Html    

    // Compute a menubar where the menu item for the given endpoint is active
    let MenuBar (ctx: Context<EndPoint>) endpoint : Doc list =
        let ( => ) txt act =
             li [if endpoint = act then yield attr.``class`` "active"] [
                a [attr.href (ctx.Link act)] [text txt]
             ]
        [
            "Home" => EndPoint.Home
            "Twitter" => EndPoint.Twitter
            "About" => EndPoint.About
        ]

    let Main ctx action (title: string) (body: Doc list) =
        Content.Page(
            Templates.MainTemplate()
                .Title(title)
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Doc()
        )
        
    let Welcome ctx (body: Doc list)  =
        Content.Page(
            Templates.LoginTemplate()
                .Title("Welcome")
                .Body(
                    div [] [client <@ Client.Login() @>]
                    )
                .Doc()
        )
        
    let Register ctx (body: Doc list)  =
        Content.Page(
            Templates.RegisterTemplate()
                .Title("Create your account")
                .Body(
                    div [] [client <@ Client.Register() @>]
                    )
                .Doc()
        )
        
module Site =
    open WebSharper.UI.Html

    let HomePage ctx =
        Templating.Main ctx EndPoint.Home "Home" [
            h1 [] [text "Say Hi to the server!"]
            div [] [client <@ Client.Main() @>]
            // div [] [client <@ Client.Test() @>]
            div [] [text "!!!!"]
        ]

    let AboutPage ctx =
        Templating.Main ctx EndPoint.About "About" [
            h1 [] [text "About"]
            p [] [text "This is a template WebSharper client-server application."]
        ]

    let Twitter ctx =
        // let userId = Var.Create ""
        // let pass = Var.Create ""
        // let myInput = Doc.Input [ attr.name "my-input" ] userId
        let userLoginDoc = 
            div [] [
                h1 [] [ text "User Login"]
                form [] [
                    input [attr.name "userId"] []
                    button [on.click (fun el ev -> JS.Alert "userId")] [text "Click!"]
                    // div [] [client <@ Client.Test() @>]
                ]
            ]
        Templating.Main ctx EndPoint.Twitter "Twitter" [
            h1 [] [text "Say Hi to the server!"]
            div [attr.id "main"] [
                form [] [
                input [attr.id "userId" ] []
                button [on.click (fun el ev -> 
                    let input = JS.Document.GetElementById("userId")
                    JS.Alert ("Alert") //(sprintf "%s" input) 
                    Client.Test()
                )] [text "Click!"]
                // div [] [client <@ Client.Test() @>]
                ]
            ]
            div [] [text "!!!!"]
        ]

        
    let WelcomePage ctx =
        Templating.Welcome ctx [
            div [] [client <@ Client.Login() @>]
        ]
        
    let RegisterPage ctx =
        Templating.Register ctx [
            div [] [client <@ Client.Register() @>]
        ]

    [<Website>]
    let Main =
        Application.MultiPage (fun ctx endpoint ->
            match endpoint with
            | EndPoint.Home -> HomePage ctx
            | EndPoint.About -> AboutPage ctx
            | EndPoint.Twitter -> Twitter ctx
            | EndPoint.Welcome -> WelcomePage ctx
            | EndPoint.Register -> RegisterPage ctx
        )
