namespace WebSharper

open FSharp.Data
open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JavaScript
open WebSharper.Json



[<JavaScript>]
module Client =

    let Main () =
        let rvReversed = Var.Create ""
        Templates.MainTemplate.MainForm()
            .OnSend(fun e ->
                async {
                    let! res = Server.DoSomething e.Vars.TextToReverse.Value
                    rvReversed := res
                }
                |> Async.StartImmediate
            )
            .Reversed(rvReversed.View)
            .OnInitDB(fun e ->
                async {
                    Server.DoDBInit ()
                }
                |> Async.StartImmediate
            )
            .Result("Sccess!")
            .Doc()
    // let Test () = 
    //     Templates.MainTemplate.MainForm()
    //         .OnTest(fun e -> Server.DoTest "TEST")
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
        let resJsonStr = Var.Create ""
        Templates.LoginTemplate.LoginBlock()
            .OnLogin(fun e ->
                let userId = e.Vars.InputUserName.Value
                let userPass = e.Vars.InputUserPass.Value
                Console.Log(userId + " : " + userPass)
                async {
                    let! res = Server.DoLogin userId userPass
                    resJsonStr := res
                }
                |> Async.StartImmediate
            )
            .Result(resJsonStr.View)
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
                    resJsonStr := res
                }
                |> Async.StartImmediate
                )
            .Result(resJsonStr.View)
            .Doc()