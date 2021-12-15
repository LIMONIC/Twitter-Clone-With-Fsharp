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
//    {
//        status: string
//        msg: string
//        content: Content
//    }
    
//
//    
//type Prop =
//    |

//[<NamedUnionCases>]
////[<JavaScript>]
//type Utils() =
//    member this.genRSA2048 (input:string) =
//        let rsa = RSA.Create(2048)
//        let padding = RSAEncryptionPadding.OaepSHA256
//        // 1. convert input string to byte array
//        // 2. generate RSA2048 key
//        rsa.Encrypt((input|> Encoding.ASCII.GetBytes), padding)
//    member this.byteArrToBase64Str (input:byte[]) = input |> Convert.ToBase64String
//    member this.base64StrToByteArr (input: string) = input |> Convert.FromBase64String

    
[<JavaScript>]
type TweetProps = {
    username: string
    content: string
}