namespace WebSharper

open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Server
open WebSharper.UI.Html    
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

    let FollowList (ctx: Context<EndPoint>) =
        async {
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            return Client.FollowList userId
        }
        |> Async.RunSynchronously

    let TweetsListString operation = 
        async {
            let res = Server.getTweetsListString operation ""
            return res
        }
        |> Async.RunSynchronously

    let TweetsList (ctx: Context<EndPoint>) operation = 
        async {
            let! session = ctx.UserSession.GetLoggedInUser()
            let userinfo = 
                match session with
                | None -> [|"";""|]
                | Some u -> u.Split(",")
            let userId = Array.get userinfo 0
            let content = Server.getTweetsList userId operation ""
            return List.map (fun cnt -> 
                let txt = cnt?text.AsString()
                let tweetId = cnt?tweetId.AsString()
                let userId = cnt?userId.AsString()
                let timestamp = cnt?timestamp.AsString()
                div [] [
                    a [attr.``class`` "list-group-item"] [
                        p [] [text ("tweetId: " + tweetId)]
                        p [] [text ("userId: " + userId)]
                        p [] [text ("Time: " + timestamp)]
                        p [] [text txt]
                    ]
                ]
            ) content
        }
        |> Async.RunSynchronously

    let Twitter2 ctx =
        Templates.TwitterTemplate.TwitterForm2()
            .TwitterList1(TweetsList ctx  "all")
            .TwitterList2(TweetsList ctx "subscribe")
            .OnTabQuery(fun e ->
                let tag = e.Vars.tabSearch.Value
                let dom = Server.getTweetsListString "tag" tag
                //TweetsListString "tab" //"mention
                
                let ele = JS.Document.GetElementById("TwitterList3")
                
                ele.InnerHTML <- dom
            )
            .OnMentionQuery(fun e ->
                let mention = e.Vars.mentionSearch.Value
                let dom = Server.getTweetsListString "mention" mention
                let ele = JS.Document.GetElementById("TwitterList4")
                ele.InnerHTML <- dom
            )
            .Doc()


    let Twitter ctx action (body: Doc list) (body2: Doc list) =
        Content.Page(
            Templates.TwitterTemplate()
                .MenuBar(MenuBar ctx action)
                .Body(body)
                .Body2(Twitter2 ctx)
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
                .FollowList(FollowList ctx)
                .Doc()
        )
        


module Site =
    open WebSharper.UI.Html

    let Twitter (ctx: Context<EndPoint>) =
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
            return! Templating.Twitter ctx EndPoint.Twitter [
                welcomeContent
                div [] [client <@ Client.Twitter() @>]
            ] [
                div [] []
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
