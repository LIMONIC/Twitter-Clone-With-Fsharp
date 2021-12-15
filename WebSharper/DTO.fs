namespace WebSharper

open WebSharper
open System
open System.Text
open System.Security.Cryptography

// post tweet
[<NamedUnionCases>]
type Props =
//    | TweetProps of content: string * hashtag: string[] * mention: string[]
    {
        content: string
        hashtag: string[]
        mention: string[]
    }
type Req =
    | Info of api: string * props: string


[<NamedUnionCases>]
type Push =
    | Info of userId: string * tweetId: string


// receive tweet
[<NamedUnionCases>]
type Content =
    | TweetContent of text: string * tweetID: string * userId: string * timestamp: string
    
type Res =
    | Info of string // status: string * msg: string * content: Content