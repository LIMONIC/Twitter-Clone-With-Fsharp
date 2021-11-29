#r "nuget: System.Data.SQLite"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
#r "nuget: Akka.TestKit"

open Akka
open Akka.Actor
open Akka.FSharp
open System
open System.Collections.Generic
open System.Data.SQLite
open System.Text
open System.Security.Cryptography
open System.IO

DateTime(2017, 07, 28, 10, 43, 31) |> printfn "%A"
let a =  System.Convert.ToString(DateTime.Now)
printfn $"aaa{DateTime.Now}"

// let getSHA1Str input  = 
//     let removeChar (stripChars:string) (text:string) =
//         text.Split(stripChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) |> String.Concat
//     System.Convert.ToString(DateTime.Now) + input
//     |> Encoding.ASCII.GetBytes
//     |> (new SHA1Managed()).ComputeHash
//     |> System.BitConverter.ToString
//     |> removeChar "-"
// let input = ""
// printfn $"aaaa{getSHA1Str }"


let databaseFilename = "Twitter.sqlite"
let curFile = @".\" + databaseFilename;
if not(File.Exists(curFile)) then
    printfn "doFile doesn't exist %s" curFile
// Console.WriteLine(File.Exists(curFile) ? "File exists." : "File does not exist.");