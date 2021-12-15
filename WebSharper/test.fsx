open System
open System.Security.Cryptography
open System.Text
(*
let byte = "Hello"B
let getSHA256Str input = 
            let removeChar (stripChars:string) (text:string) =
                text.Split(stripChars.ToCharArray(), StringSplitOptions.RemoveEmptyEntries) |> String.Concat
            System.Convert.ToString(DateTime.Now) + input
            |> Encoding.ASCII.GetBytes
            |> (new SHA256Managed()).ComputeHash
            |> System.BitConverter.ToString
            |> removeChar "-"
let getHashKey (input: byte[]) =
    
    let hmac = new HMACSHA256(byte)
    let hmacn = new HMACSHA256()
    printfn "%A" byte
    printfn "%A" hmac.Key
    printfn "%A" (Convert.ToBase64String(hmac.ComputeHash(byte)))
    printfn "%s" (Convert.ToBase64String(hmac.Key))
    hmac.ComputeHash(input)

// *)
let genRSA2048 =
    let rsa = RSA.Create(2048)
    let padding = RSAEncryptionPadding.OaepSHA256
    let sigPadding = RSASignaturePadding.Pkcs1
    let publicKey = rsa.Encrypt(byte, padding)
    printfn "publicKey: \n%s" (Convert.ToBase64String(publicKey))
    let algName = HashAlgorithmName.SHA256
    
    let signiture = rsa.SignHash(getHashKey publicKey, algName, sigPadding)
    let dataSign = rsa.SignData("hello"B, algName, sigPadding)
    signiture|> printfn "Sign:  %A"
    rsa.VerifyHash(getHashKey publicKey, signiture, algName, sigPadding) |> printfn "Verify Sign:  %A"
    rsa.VerifyData("hello"B, dataSign, algName, sigPadding) |> printfn "Verify data Sign:  %A"

    
let mutable alicePublicKey = null
let mutable bobPublicKey = null
let hmacKey = "hello"B
let bob = ECDiffieHellmanCng.Create()
let DHBob =
    
    bob.KeyExchangeAlgorithm = CngAlgorithm.Sha256.Algorithm

    let algName = HashAlgorithmName.SHA256
    bobPublicKey <- bob.PublicKey
    bob.PublicKey.ToByteArray() |> Convert.ToBase64String |> printfn "bobPublicKey %A"
let DHAlice =
    let alice = ECDiffieHellmanCng.Create()
    alice.KeyExchangeAlgorithm = CngAlgorithm.Sha256.Algorithm

    let algName = HashAlgorithmName.SHA256
    alicePublicKey <- alice.PublicKey
    alice.PublicKey.ToByteArray() |> Convert.ToBase64String |> printfn "alicePublicKey %A"
    let aliceKey = alice.DeriveKeyFromHmac(bobPublicKey, algName, hmacKey) // hmacKey -> public key
    printfn "alice private key: \n%A" (Convert.ToBase64String(aliceKey))

let DHBob1 =
    let algName = HashAlgorithmName.SHA256
    let bobKey = bob.DeriveKeyFromHmac(alicePublicKey, algName, hmacKey)
    printfn "bob private key: \n%A" (Convert.ToBase64String(bobKey))
//    let bobKey = alice.DeriveKeyFromHmac(bobsKey, algName, hmacKey) // hmacKey -> public key

//getHashKey
//genRSA2048
DHBob
printfn "bobPublicKey: %A" bobPublicKey
DHAlice

//getSHA256Str "qwerty" |> printfn "%A"

// 注册时，根据用户信息，基于RSA-2048，生成public key， 存入到password字段
// 登录、WebSocket 认证步骤
// 1. server 发送 challenge 256-bit 字符串
// 2. client 返回
//      a. challenge
//      b. UNIX time
//      c. signiture -> 使用自己的public key （RSA）
// 3. Server 校验
//      a. 检查challenge是否一致
//      b. 检查UNIX time是否超时
//      c. verify signature （RSA）
//      d. 返回校验结果 // success | failed | try again
//         - 成功时返回 Diffie-Hellman public Key
//4. client 响应 （Alice）
//    -成功时
//      a. 生成自己的 Diffie-Hellman public key
//      b. 根据 server 的 public key 以及 hmacKey（就是最早的RSA-2048 key） 协商出自己的private key
//      c. 将自己的 Diffie-Hellman public key 发送给客户端
//      d. 对同一session下的所有信息 使用private key 基于RSA算法 签名
//5. server 协商 private key
//      a. 根据 client 的 public key 以及 hmacKey（就是最早的RSA-2048 key） 协商出自己的private key
//      b. 对于所有从client 接收到的信息，使用private key 校验签名。不通过返回错误
 

// 
//数据传输验证
