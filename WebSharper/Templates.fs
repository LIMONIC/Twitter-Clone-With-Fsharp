namespace WebSharper

open WebSharper.UI.Templating
open WebSharper.UI.Notation
open FSharp.Data
open FSharp.Data.JsonExtensions

[<JavaScript>]
module Templates =
    type MainTemplate = Template<"./templates/Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type LoginTemplate = Template<"./templates/login.html" , ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type RegisterTemplate = Template<"./templates/register.html" , ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type TwitterTemplate = Template<"./templates/Twitter.html" , ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type AccountTemplate = Template<"./templates/account.html" , ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type TwitterDemoTemplate = Template<"./templates/TwitterDemoPanel.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>