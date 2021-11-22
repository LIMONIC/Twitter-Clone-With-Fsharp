# Twitter-Clone-With-Fsharp

### Json

```sql
// Tweet Req:
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":"1985"
    },
    "props":{
        "content":"test001",
        "hashtag": ["tag1", "tag2"],
				"mention": ["mention1", "mention2"]
    }
}

// ReTweet Req:
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":"1985"
    },
    "props":{
        "tweetId":"test001",
        //"hashtag":"test001@test.com",
				//"mention":""
    }
}

// follow:
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":"1985"
    },
    "props":{
        "userId":"test001"
    }
}

// unfollow:
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":"1985"
    },
    "props":{
        "userId":"test001"
    }
}

// query
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":"1985"
    },
    "props":{
        "operation":"" // subscribe || tag || mention || all
    }
}
```




### Server-Client JSON

Register

```json
// Success
------------Request-------------
{
  "api": "Register",
  "auth": {
    "id": "Admin006",
    "password": "Admin006"
  },
  "props": {
    "email": "test6@test.com",
    "nickName": "hannah"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Registration Success. Your user id is Admin006.",
  "content": []
}
--------------------------------

// User exist
------------Response------------
{
  "status": "error",
  "msg": "UserId has already been used. Please choose a new one.",
  "content": []
}
--------------------------------
```

Login

```json
// success
------------Request-------------
{
  "api": "Login",
  "auth": {
    "id": "Admin006",
    "password": "Admin006"
  },
  "props": {
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Login Success. You have logged in as Admin006",
  "content": []
}
--------------------------------
// wrong userid
------------Request-------------
{
  "api": "Login",
  "auth": {
    "id": "Admin007",
    "password": "Admin006"
  },
  "props": {
  }
}
------------Response------------
{
  "status": "error",
  "msg": "User not existed. Please check the user information",
  "content": []
}
--------------------------------
// duplicated login
------------Request-------------
{
  "api": "Login",
  "auth": {
    "id": "Admin006",
    "password": "Admin00666"
  },
  "props": {
  }
}
------------Response------------
{
  "status": "success",
  "msg": "User Admin006 has already logged in.",      
  "content": []
}
--------------------------------
// wrong pass 
------------Request-------------
{
  "api": "Login",
  "auth": {
    "id": "Admin005",
    "password": "Admin00666"
  },
  "props": {
  }
}
------------Response------------
{
  "status": "error",
  "msg": "Login Failed. Please try again.",
  "content": []
}
--------------------------------
```

Tweet

- `hashtag` `mention` are optional
- multiple hashtags and/or mentions are allowed

```json
------------Request-------------
{
  "api": "Tweet",
  "auth": {
    "id": "Admin006",
    "password": "Admin006"
  },
  "props": {
    "content": "Suave is a simple web development F#",
    "hashtag": [
      "Suave",
      "test7"
    ],
    "mention": [
      "Admin001",
      "Admin002"
    ]
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Tweet sent.",
  "content": []
}
--------------------------------
```

Retweet

- `hashtag` `mention` are optional
- multiple hashtags and/or mentions are allowed

```json
------------Request-------------
{
  "api": "ReTweet",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "tweetId": "0F135E30DCF16D471B8219B55431B61F9321F884"
  },
  "hashtag": [
    "Suave",
    "test8"
  ],
  "mention": [
    "Admin003",
    "Admin004"
  ]
}
------------Response------------
{
  "status": "success",
  "msg": "Retweet success.",
  "content": []
}
--------------------------------
```