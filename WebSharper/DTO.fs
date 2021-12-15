namespace WebSocketExample

open WebSharper

[<NamedUnionCases>]
type C2SMessage =
    | Speak of content: string
    
[<NamedUnionCases>]
type S2CMessage =
    | Broadcast of value: string
    
[<JavaScript>]
type TalkingInfo = {
    username: string
    content: string
}