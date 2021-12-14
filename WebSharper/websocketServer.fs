module WebSharper.AspNetCore.Tests.WebSocketServer

open WebSharper
open WebSharper.AspNetCore.WebSocket.Server
open System.Collections.Generic;
open System
open System.Threading
open System.Threading.Tasks
open System.Net.Sockets
open System.IO
open System.Linq;
open Newtonsoft.Json

type [<JavaScript; NamedUnionCases>]
    C2SMessage =
    | Register of reg: string
    | Login of randomInt:int
    | Logout of randomInt2: int


and [<JavaScript; NamedUnionCases "type">]
    S2CMessage =
    | [<Name "string">] CommonResponse of value: string
    | [<Name "string">] RegisterResponse of value: string
    | [<Name "string">] LoginResponse of value: string
    | [<Name "string">] LogoutResponse of value: string
    | [<Name "string">] ErrorResponse of value:string
[<System.SerializableAttribute>]

type User() = 
    [<DefaultValue>]
    val mutable Name : string
    [<DefaultValue>]
    val mutable Status : bool
    [<DefaultValue>]
    val mutable Feeds : List<Tweet>
    [<DefaultValue>]
    val mutable SubscribersList : HashSet<string>


let Start() : StatefulAgent<S2CMessage, C2SMessage, int> =
    let mutable users = new Dictionary<string, User>()
    let mutable userGuIdMapper = new Dictionary<string, string>()
    let mutable guIdUserMapper =  new Dictionary<string, string>()
    let mutable closedConnectionList =  new List<string>()
    let mutable totalUsers  = 0
    let mutable totalOnlineUsers = 0
    let mutable totalOfflineUsers = 0
    let mutable totalTweets = 0
    
    let mutable clientTaskList : (string  * WebSocketClient<S2CMessage,C2SMessage>) list = []
    
    let addClientTask cl = clientTaskList <- cl :: clientTaskList
    
    
    // Used to communicate with the client.
    let writeToClient (cl:WebSocketClient<S2CMessage,C2SMessage>, message ) =
        async{
            do! cl.PostAsync(message)
        }


    // Used to create new user object.
    let createUser(name: string) = 
        let user = User()
        user.Name <- name
        user.Status <-  true
        user.Feeds <- new List<Tweet>()
        user.SubscribersList <- new HashSet<string>()
        user

    
    // Creates a socket connection with the client.
    fun client -> async {
        let clientIp = client.Connection.Context.Connection.RemoteIpAddress.ToString()
        let clientId =  System.Guid.NewGuid().ToString()
        addClientTask( clientId, client)
        return 0, fun state msg -> async {
            dprintfn "Received message #%i from %s" state clientId
            // This is used perform the required actions on the inmemory database(we are using lists) to communicate with the client.
            match msg with
            | Message data -> 
                match data with
                | Register userName ->
                    // If user is already present then donot add to db else add and acknowledge.
                    if(users.ContainsKey(userName)) then
                        do! sendMessageToClient(clientId, ErrorResponse "Error: Error while creating user. User already present") |> Async.Ignore
                        dprintfn "Ignoring request as user with id is already present"
                    else if (guIdUserMapper.ContainsKey(clientId)) then
                        do! sendMessageToClient(clientId, ErrorResponse "Error: You can only create a single user with single socket connection. User already present with this client. Open new tab for creating new user.") |> Async.Ignore
                        dprintfn "Ignoring request as user with id is already present"
                    else
                        let user = createUser userName
                        users.Add(userName, user) |> ignore
                        userGuIdMapper.Add(userName,clientId) |> ignore
                        guIdUserMapper.Add(clientId,userName) |> ignore
                        totalUsers <- totalUsers+1
                        totalOnlineUsers <- totalOnlineUsers+1
                        do! writeToClient(client, RegisterResponse "Successfully registered.")
                | Login ints ->
                    // If not logged in send the previous tweets else if user is already logged in then do nothing.
                    let user =  users.[guIdUserMapper.[clientId]]
                    if(not user.Status) then
                        user.Status <- true
                        totalOnlineUsers <- totalOnlineUsers + 1
                        totalOfflineUsers <- totalOfflineUsers - 1
                        do! writeToClient(client, ErrorResponse  "You are now logged in.")
                        if (user.Feeds.Count > 0) then
                            for i in user.Feeds do
                               do! sendMessageToClient(clientId, (TweetResponse(JsonConvert.SerializeObject(i)))) |> Async.Ignore
                        user.Feeds <-  new List<Tweet>()
                    else
                        do! writeToClient(client, ErrorResponse  "You are already logged in.")
                | Logout  ints->
                    // If logged in log him/her out else do nothing.
                    let user = users.[guIdUserMapper.[clientId]]
                    if user.Status then
                        user.Status <- false
                        totalOfflineUsers <- totalOfflineUsers + 1
                        totalOnlineUsers <- totalOnlineUsers - 1
                        do! writeToClient(client, CommonResponse  "You are logged out.")
                    else 
                        do! writeToClient(client, CommonResponse  "You are already logged out.")
                
            | Error exn -> 
                eprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                do! client.PostAsync (CommonResponse ("Error: " + exn.Message))
                return state
            | Close ->
                closedConnectionList.Add(clientId)
                dprintfn "Closed connection to %s" clientIp
                return state
        }
    }