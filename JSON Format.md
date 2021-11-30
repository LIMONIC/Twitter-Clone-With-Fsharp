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

Follow

```json
// success
------------Request-------------
{
  "api": "Follow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin001"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Admin004 successfully followed Admin001.",
  "content": []
}
--------------------------------
// user to follow not exist
------------Request-------------
{
  "api": "Follow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin0011"
  }
}
------------Response------------
{
  "status": "error",
  "msg": "User Admin0011 not exist. Please check the user information",
  "content": []
}
--------------------------------
// already followed
------------Request-------------
{
  "api": "Follow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin003"
  }
}
------------Response------------
{
  "status": "error",
  "msg": "User Admin004 already followed user Admin003.",      
  "content": []
}
--------------------------------
```

unfollow

```json

// success
------------Request-------------
{
  "api": "UnFollow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin001"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Admin004 successfully unfollowed Admin001.",
  "content": []
}
--------------------------------
// not following
------------Request-------------
{
  "api": "UnFollow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin003"
  }
}
------------Response------------
{
  "status": "error",
  "msg": "User Admin004 is not following user Admin003.",      
  "content": []
}
--------------------------------
// user not exist
------------Request-------------
{
  "api": "UnFollow",
  "auth": {
    "id": "Admin004",
    "password": "Admin004"
  },
  "props": {
    "userId": "Admin009"
  }
}
------------Response------------
{
  "status": "error",
  "msg": "User Admin009 not exist. Please check the user information",
  "content": []
}
--------------------------------
```

Query

```json
// All
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "all"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "The latest 20 tweets:",
  "content": [
    {
      "text": "Nice weather balabala!",
      "tweetId": "A813A096C85E37AA6890073A02576503ECA5015C",   
      "userId": "Admin004",
      "timestamp": "2021-11-22T17:18:37"
    },
    {
      "text": "Suave is a simple web development F#",
      "tweetId": "B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD",   
      "userId": "Admin006",
      "timestamp": "2021-11-22T17:14:10"
    },
    {
      "text": "Nice weather bala...
--------------------------------
// subscribe - no result
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "subscribe"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "user Admin002's subscribed tweets:",
  "content": []
}
--------------------------------
// subscribe
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "subscribe"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "user Admin002's subscribed tweets:",
  "content": [
    {
      "text": "Nice weather balabala!",
      "tweetId": "A813A096C85E37AA6890073A02576503ECA5015C",   
      "userId": "Admin004",
      "timestamp": "2021-11-22T17:18:37"
    },
    {
      "text": "Suave is a simple web development F#",
      "tweetId": "B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD",   
      "userId": "Admin006",
      "timestamp": "2021-11-22T17:14:10"
    }
  ]
}
--------------------------------
// tag
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "tag",
    "tagId": "Suave"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Tweets related to tag Suave:",
  "content": [
    {
      "text": "Suave is a simple web development F#",
      "tweetId": "B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD",   
      "userId": "Admin006",
      "timestamp": "2021-11-22T17:14:10"
    }
  ]
}
--------------------------------
// mention-user id
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "mention",
    "mention": "Admin001"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Tweets related to user Admin001:",
  "content": [
    {
      "text": "Suave is a simple web development F#",
      "tweetId": "B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD",   
      "userId": "Admin006",
      "timestamp": "2021-11-22T17:14:10"
    },
    {
      "text": "Nice weather balabala!",
      "tweetId": "0F135E30DCF16D471B8219B55431B61F9321F884",   
      "userId": "Admin005",
      "timestamp": "2021-11-21T23:38:14"
    },
    {
      "text": "Nice w...
--------------------------------
// mention-nick name
------------Request-------------
{
  "api": "Query",
  "auth": {
    "id": "Admin002",
    "password": "Admin002"
  },
  "props": {
    "operation": "mention",
    "mention": "Bob"
  }
}
------------Response------------
{
  "status": "success",
  "msg": "Tweets related to user Bob:",
  "content": [
    {
      "text": "Suave is a simple web development F#",
      "tweetId": "B85C72BE1FB410E6CDF9A806B3C4CA6FAEE7D5DD",   
      "userId": "Admin006",
      "timestamp": "2021-11-22T17:14:10"
    },
    {
      "text": "Nice weather balabala!",
      "tweetId": "0F135E30DCF16D471B8219B55431B61F9321F884",   
      "userId": "Admin005",
      "timestamp": "2021-11-21T23:38:14"
    }
  ]
}
--------------------------------
```