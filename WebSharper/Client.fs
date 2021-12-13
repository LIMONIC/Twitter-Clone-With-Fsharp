namespace WebSharper

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
                    if status = "success" then 
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
                let userPass = e.Vars.RegisterUserPass.Value
                let userEmail = e.Vars.RegisterUserEmail.Value
                let prop = (sprintf """{"email": "%s", "nickName": "%s"}""" userEmail userId)
                Console.Log(userId + " : " + userPass + " : " + userEmail)
                async {
                    let! res = Server.DoRegister userId userPass prop
                    let resobj = JSON.Parse(res)
                    let status = resobj?status
                    if status = "success" then 
                        JS.Window.Location.Replace("/")
                }
                |> Async.StartImmediate
                )
            .Doc()

    let Twitter () =
        let resJsonStr = Var.Create ""
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
                }
                |> Async.StartImmediate
            )
            .Result(resJsonStr.View)
            .Doc()


    