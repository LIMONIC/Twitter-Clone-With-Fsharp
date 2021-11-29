#r "nuget: FSharp.Data"

open System
open FSharp.Data
open FSharp.Data.JsonExtensions

let info =
  JsonValue.Parse(""" 
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":1985
    },
    "props":{
        "content":"test001",
        "hashtag":["test001@test.com", "tag2"],
				"mention":""
    }
} """)


let infoEmpty =
  JsonValue.Parse("""{}""")
// printfn "%A" (info?props) 
let n = info?props?hashtag.AsArray()
// printfn "%A" n.Length
[for v in n -> 
  printfn $"{v}"
  printfn $"{v.AsString()}"
  ] 
// printfn "%s (%d)" (info?name.AsString()) (info?born.AsInteger())


// https://yukitos.github.io/FSharp.Data/library/JsonValue.html

// 1．看到所有的tweets
// tweets：content + user


// """ 
//     { "id": "Tomas", "pass": 1985,
//       "siblings": [ "Jan", "Alexander" ] } """
