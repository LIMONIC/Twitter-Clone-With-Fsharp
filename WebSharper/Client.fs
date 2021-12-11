namespace WebSharper

open WebSharper
open WebSharper.UI
open WebSharper.UI.Templating
open WebSharper.UI.Notation
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.JavaScript


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
        let userId = Var.Create ""
        let userPass = Var.Create ""
        Templates.LoginTemplate.LoginBlock()
            .OnLogin(fun e ->
                let id = e.Vars.InputUserName.Value
                let pass = e.Vars.InputUserPass.Value
                Console.Log(id + " : " + pass)
//            async {
//                let! res = Server.DoLogin e.Vars.userId.Value
//                resStr := res
//            }
//            |> Async.StartImmediate
            )
            .Doc()
        