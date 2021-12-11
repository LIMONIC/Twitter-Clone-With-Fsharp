namespace WebSharper

open WebSharper.UI.Templating
open WebSharper.UI.Notation

[<JavaScript>]
module Templates =
    type MainTemplate = Template<"./templates/Main.html", ClientLoad.FromDocument, ServerLoad.WhenChanged>
    type LoginTemplate = Template<"./templates/login.html" , ClientLoad.FromDocument, ServerLoad.WhenChanged>
    