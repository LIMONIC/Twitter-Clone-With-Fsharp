# Twitter-Clone-With-Fsharp

### Json

```sql
// Tweet Req:
{
    "api":"",
    "auth":{
        "id":"t10000",
        "password":1985
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
        "password":1985
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
        "password":1985
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
        "password":1985
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
        "password":1985
    },
    "props":{
        "target":"" // tag || mention
    }
}
```