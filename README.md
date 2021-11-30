# **Project Files Overview:**

## **Files:**

**client.fsx**: Contains client and front-end logic where each client corresponds to a single user and performs all the functions for a user. Require IP address and port.

**server.fsx**: Contains all the logic related to user authentication, tweet and retweet processing, hashtag and mention handling, user follow and unfollow handling, DB operation etc.

**simulate**.fsx: Contains all the logic related to register users, periods of live connection and disconnection for users, follow other users, increase the number of tweets, make some of these messages re-tweets.

**Twitter.sqlite**: database file.

**README.pdf**

## **How to Run:**

```bash
dotnet fsi  server.fsx
dotnet fsi  client.fsx <IP_Address> <Port>
```

- IP_Address: input the client’s ip address
- Port: input the client’s port

### For simulation:

```bash
dotnet fsi tester.fsx
```

## System Design:

The system is designed in a form of client-end and back-end separation. The two ends communicates to each other with a lightweight data-interchange format, JSON, encapsulated in two types for sending requests and receiving responses. 

The system is divided based on the functions by making each actor focus on one service. This improves the scalability and maintainability of the system.

A lightweight relational database is employed for data OCRD. The structure of the database is optimized for the system functionalities.  

![Fig 1. System architecture of the twitter clone system.](https://github.com/LIMONIC/Twitter-Clone-With-Fsharp/blob/main/img/image.jpg)

Fig 1. System architecture of the twitter clone system.

## **Functionalities implemented:**

### System

- **Register**: Users can register themselves to Twitter
- **User authentication**: Users can login to or logout from the Twitter (live connection / disconnection) with username and password
- **Get tweets in live**: Users can open the Tweets Page to receive tweets lively. This means users who have logged in the system will receive tweets simultaneously when the user they are following make tweets.
- **Follow and unfollow other users**: Users can follow and unfollow another user already registered and pop out a warning if the user doesn’t exist.
- **Send Tweet**: Users can send tweets with / without multiple hashtags and / or multiple mentions.
- **Retweet**: Users can retweet the tweets posted by other users according to the tweet ID.
- **Query tweets**: Users can query to see tweets posted by the users they followed, and can also search for related tweets based on mention and tag.

### Test

- Implement a tester to test the above functions.
- The followers are distributed using the Zipf distribution. There are 3 types of user: celebrity, influencer and common user. The celebrity’s followers number will be twice the time of the influencer’s and will be the third time of the common user’s.
- celebrities and influencers will tweet many msg and common users will retweet some of them from the user they followed.
- Simulate periods of live connection and disconnection for users.

## **Simulation Results**

### **For 300 Users**
| For 300 Users | Users | Followers | Avg. Followers | Tweets & Retweets | Avg. Tweets |
| ------------- | ----- | --------- | -------------- | ----------------- | ----------- |
| celebrity     | 1     | 142       | 479            | 479               | 479         |
| influencer    | 3     | 201       | 405            | 135               | 135         |
| common user   | 296   | 10360     | 4992           | 16                | 16          |
| Total         | 300   |           |                |                   |             |

Offline users : 15.0% = 45 (Chosen randomly between 10% - 20% of total users)

Total time taken :  534.002

### **For 500 Users**

| For 500 Users | Users | Followers | Avg. Followers | Tweets & Retweets | Avg. Tweets |
| ------------- | ----- | --------- | -------------- | ----------------- | ----------- |
| celebrity     | 1     | 284       | 284            | 517               | 517         |
| influencer    | 3     | 240       | 80             | 540               | 180         |
| common user   | 496   | 12335     | 24             | 8614              | 17          |
| Total         | 500   |           |                |                   |             |

Offline users : 18.0% = 90 (Chosen randomly between 10% - 20% of total users)

Total time taken :  700.071

### **For 1000 Users**

| For 1000 Users | Users | Followers | Avg. Followers | Tweets & Retweets | Avg. Tweets |
| -------------- | ----- | --------- | -------------- | ----------------- | ----------- |
| celebrity      | 1     | 503       | 503            | 547               | 547         |
| influencer     | 5     | 1120      | 224            | 566               | 113         |
| common user    | 994   | 79520     | 80             | 16792             | 16          |
| Total          | 1000  |           |                |                   |             |

Offline users : 17.0% = 170 (Chosen randomly between 10% - 20% of total users)

Total time taken :  1438.099

### **For 5000 Users**

| For 5000 Users | Users | Followers | Avg. Followers | Tweets & Retweets | Avg. Tweets |
| -------------- | ----- | --------- | -------------- | ----------------- | ----------- |
| celebrity      | 5     | 7423      | 1484           | 1459              | 291         |
| influencer     | 25    | 16488     | 659            | 2914              | 116         |
| common user    | 4970  | 1231347   | 247            | 99400             | 20          |
| Total          | 5000  |           |                |                   |             |

Offline users : 15.0% = 750 (Chosen randomly between 10% - 20% of total users)

Total time taken :  8328.574

### **For 10000 Users**

| For 10000 Users | Users | Followers | Avg. Followers | Tweets & Retweets | Avg. Tweets |
| --------------- | ----- | --------- | -------------- | ----------------- | ----------- |
| celebrity       | 10    | 27854     | 2785           | 2682              | 268         |
| influencer      | 50    | 64101     | 1282           | 5760              | 115         |
| common user     | 9940  | 5001756   | 503            | 169433            | 17          |
| Total           | 10000 |           |                |                   |             |

Offline users : 18.0% = 1800 (Chosen randomly between 10% - 20% of total users)

Total time taken :  22171.157