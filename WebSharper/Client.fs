namespace WebSharper

open System
open System.Text
open System.Security.Cryptography
open FSharp.Data
open FSharp.Data.JsonExtensions
open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.Sitelets
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JavaScript
open WebSharper.Json
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client
  
  
[<JavaScript>]
module Client =

    let LoggedInUser () =
        div [attr.style "margin-bottom: 20px"] [
            p [attr.style "display: inline; margin: 0 10px 15px 0"] [text "click here to log out:"]
            button [
                attr.``class`` "btn btn-primary mybtn"
                on.click (fun _ _ ->
                    async {
                        let! res = Server.DoLogout()
                        return JS.Window.Location.Replace("/")
                    }
                    |> Async.Start
                )
            ] [text "logout"]
        ]

    let guest() =
        div [attr.style "margin-bottom: 20px"] [
            p [attr.style "display: inline; margin: 0 10px 15px 0"] [text "click here to log in:"]
            button [
                attr.``class`` "btn btn-primary mybtn"
                on.click (fun _ _ ->
                    async {
                        Console.Log("login")
                        return JS.Window.Location.Replace("/welcome")
                    }
                    |> Async.Start
                )
            ] [text "login"]
        ]

    // let Main () =
    //     let rvReversed = Var.Create ""
    //     Templates.MainTemplate.MainForm()
    //         .OnSend(fun e ->
    //             async {
    //                 let! res = Server.DoSomething e.Vars.TextToReverse.Value
    //                 rvReversed := res
    //             }
    //             |> Async.StartImmediate
    //         )
    //         .Reversed(rvReversed.View)
    //         .OnInitDB(fun e ->
    //             async {
    //                 Server.DoDBInit ()
    //             }
    //             |> Async.StartImmediate
    //         )
    //         .Result("Sccess!")
    //         .Doc()
       
    let Db () = 
        Templates.MainTemplate.MainForm()
            .OnInitDB(fun e ->
                async {
                    Server.DoDBInit ()
                }
                |> Async.StartImmediate
            )
            .Result("Sccess!")
            .Doc()

    let Test () = 
        let input = Var.Create ""
        let inputField = Doc.Input [] input
        // let label = textView input.View
        let copyTheInput =
            div [attr.``class`` "panel-default"] [
              div [attr.``class`` "panel-body"] [
                inputField
                // div [label]
              ]
            ]
        Doc.RunById "main" copyTheInput
        // Templates.MainTemplate.MainForm()
        //     .OnLogin(fun e ->
        //         async {
        //             let! res = Server.DoLogin e.Vars.userId.Value
        //             resStr := res
        //         }
        //         |> Async.StartImmediate
        //     )
        //     .Result(resStr.View)
        //     .Doc()
   
    let Login () =
        Templates.LoginTemplate.LoginBlock()
            .OnLogin(fun e ->
                let userId = e.Vars.InputUserName.Value
                let userPass = e.Vars.InputUserPass.Value
                Console.Log(userId + " : " + userPass)
                async {
                   
                    let! res = Server.DoLogin userId userPass
                    let resobj = JSON.Parse(res)
                    let status = resobj?status
                    Console.Log(status)
                    if status = "success" then 
                        Console.Log("111")
                        JS.Window.Location.Replace("/")
                }
                |> Async.StartImmediate
            )
            .Doc()

    let Register () =
        let resJsonStr = Var.Create ""
        Templates.RegisterTemplate.LoginBlock()
            .OnRegister(fun e ->
                let userId = e.Vars.RegisterUserName.Value
//                let userCredential =  e.Vars.RegisterUserPass.Value |> genRSA2048 |> byteArrToBase64Str
                let userPass = e.Vars.RegisterUserPass.Value
                let userEmail = e.Vars.RegisterUserEmail.Value
                let prop = (sprintf """{"email": "%s", "nickName": "%s"}""" userEmail userId)
                Console.Log(userId + " : " + userPass + " : " + userEmail)
                async {
                    let! res = Server.DoRegister userId userPass prop
                    let resObj = JSON.Parse(res)
                    let status = resObj?status
                    if status = "success" then 
                        JS.Window.Location.Replace("/")
                }
                |> Async.StartImmediate
                )
            .Doc()

    
    let FollowList userId =
        let array = Server.getFollowersList userId 
        
        List.map (fun txt -> 
            div [] [
                a [attr.``class`` "list-group-item"] [text txt]
            ]
        ) array

    
      

    let Account () =
        let resJsonStr = Var.Create ""
        Templates.AccountTemplate.AccountForm()
            .OnFollow(fun e ->
                let followID = e.Vars.followID.Value
                
                Console.Log(followID)
                async {
                    let! res = Server.DoFollow followID
                    Console.Log(res)
                    JS.Window.Location.Reload()
                }
                |> Async.StartImmediate
            )
            .OnUnfollow(fun e ->
                let unfollowID = e.Vars.unfollowID.Value
                
                Console.Log(unfollowID)
                async {
                    let! res = Server.DoUnfollow unfollowID
                    Console.Log(res)
                    JS.Window.Location.Reload()
                }
                |> Async.StartImmediate
            )
            
            .Doc()
    let Twitter(ep) =
//    let Twitter (ep: WebSocketEndpoint<Res, Push>) =
        let resJsonStr = Var.Create ""
        let mutable wsServiceProvider: WebSocketServer<Res,Push> option = None
//        let cardComponent userId text tweetId time = 
//            div [attr.``class`` "card"; attr.style "width: 50rem;"] [
//                div [attr.``class`` "card-body"] [
//                    h5 [attr.``class`` "card-title"] [userId]
//                    p [attr.``class`` "card-text"] [text]
//                ]
//                div [attr.``class`` "card-footer"] [
//                    ul [attr.``class`` "list-group list-group-flush"] [
//                        li [attr.``class`` "list-group-item fs-6 fw-light"] [tweetId]
//                        li [attr.``class`` "list-group-item fs-6 fw-light"] [time]
//                    ]
//                ]
//            ]
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
                                let content = try resObj?content with _-> [||]
//                                let card = cardComponent
//                                               content.[0]?userId
//                                               content.[0]?text
//                                               content.[0]?tweetId
//                                               content.[0]?timestamp
                                
                                let div = JS.Document.GetElementById("tweetsDemoPanel")
                                let liTweetId = JS.Document.CreateElement("li")
                                liTweetId.SetAttribute("class", "list-group-item fs-6 fw-light")
                                liTweetId.AppendChild(JS.Document.CreateTextNode(content.[0]?tweetId)) |> ignore
                                let liTimestamp = JS.Document.CreateElement("li")
                                liTimestamp.SetAttribute("class", "list-group-item fs-6 fw-light")
                                liTimestamp.AppendChild(JS.Document.CreateTextNode(content.[0]?timestamp)) |> ignore
                                let ul = JS.Document.CreateElement("ul")
                                ul.SetAttribute("class", "list-group list-group-flush")
                                ul.AppendChild(liTweetId) |> ignore
                                ul.AppendChild(liTimestamp) |> ignore
                                let divCardFoot = JS.Document.CreateElement("div")
                                divCardFoot.SetAttribute("class", "card-footer")
                                divCardFoot.AppendChild(ul) |> ignore
                                
                                let cardTil = JS.Document.CreateElement("h5")
                                cardTil.SetAttribute("class", "card-title")
                                cardTil.AppendChild(JS.Document.CreateTextNode(content.[0]?userId)) |> ignore
                                let cardText = JS.Document.CreateElement("p")
                                cardText.SetAttribute("class", "card-text")
                                cardText.AppendChild(JS.Document.CreateTextNode(content.[0]?text)) |> ignore
                                let divCardBody = JS.Document.CreateElement("div")
                                divCardBody.SetAttribute("class", "card-body")
                                divCardBody.AppendChild(cardTil) |> ignore
                                divCardBody.AppendChild(cardText) |> ignore
                                
                                let divCard = JS.Document.CreateElement("div")
                                divCard.SetAttribute("class", "card")
                                divCard.SetAttribute("style", "width: 40rem;")
                                divCard.AppendChild(divCardBody) |> ignore
                                divCard.AppendChild(divCardFoot) |> ignore

                                div.AppendChild(divCard) |> ignore
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
        
        Templates.TwitterTemplate.TwitterForm()
            .OnTweet(fun e ->
                let content = e.Vars.tweetContent.Value
                let tag = e.Vars.tweetTags.Value.Split [|','|]
                let tags = "\"" + System.String.Join("\",\"", tag) + "\""
                let mention = e.Vars.tweetMentions.Value.Split [|','|]
                let mentions = "\"" + System.String.Join("\",\"", mention) + "\""
                let prop = (sprintf """{"content": "%s", "tag": [%s], "mention": [%s]}""" content tags mentions)
                Console.Log(prop)
                async {
                    let! res = Server.DoTweet prop
                    resJsonStr := res
                    JS.Window.Location.Reload() 
                    // push tweet to followers and mentioned users
                    let resObj = JSON.Parse res
                    if resObj?status = "success" then
                        let contentArr = try resObj?content with _->[||]
                        let userId = contentArr.[0]?userId
                        let tweetId = contentArr.[0]?tweetId
                        wsServiceProvider.Value.Post(Push.Info (userId, tweetId))
                }
                |> Async.StartImmediate
            )
            .OnReTweet(fun e ->
                let tweetId = e.Vars.tweetID.Value
                let tag = e.Vars.reTweetTags.Value.Split [|','|]
                let tags = "\"" + System.String.Join("\",\"", tag) + "\""
                let mention = e.Vars.reTweetMentions.Value.Split [|','|]
                let mentions = "\"" + System.String.Join("\",\"", mention) + "\""
                let prop = (sprintf """{"tweetId": "%s", "tag": [%s], "mention": [%s]}""" tweetId tags mentions)
                Console.Log(prop)
                async {
                    let! res = Server.DoReTweet prop
                    resJsonStr := res
                    JS.Window.Location.Reload() 
                    // push tweet to followers and mentioned users
                    let resObj = JSON.Parse res
                    if resObj?status = "success" then
                        let contentArr = try resObj?content with _->[||]
                        let userId = contentArr.[0]?userId
                        let tweetId = contentArr.[0]?tweetId
                        wsServiceProvider.Value.Post(Push.Info (userId, tweetId))
                }
                |> Async.StartImmediate
            )
//            .Result(resJsonStr.View)
            .Doc()


    