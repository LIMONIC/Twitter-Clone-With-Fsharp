# **Twitter Clone with F#**

> **Video Link:**
[https://youtu.be/kuVMFMufXHg](https://youtu.be/kuVMFMufXHg)

## Introduction

This project uses F# and WebSharper framework to implements functionalities of Twitter. It consists of front end and back end.

For the front-end, functions are split on multiple pages. Navbar and tab bar are used to differentiate those functions. In addition, special pages are set up for login and registration operations. Pages can be routed by user actions.

In addition, JSON format are designed between the front and back ends to make it easy to pass the corresponding information according to different requests.

for the backend, we set different API interfaces for different requests and used SQLite database to manage the information in a unified way. For displaying tweets, we upgrade the http connection to websocket to achieve real-time pushing of tweets from the server to the client.

## System Design:

The system is designed in a form of client-end and back-end separation. The two ends communicates to each other with a lightweight data-interchange format, JSON, encapsulated in two types for sending requests and receiving responses.

The system is divided based on the functions by making each actor focus on one service. This improves the scalability and maintainability of the system.

A lightweight relational database is employed for data OCRD. The structure of the database is optimized for the system functionalities.

![Fig 1. System architecture of the twitter clone system.](https://github.com/LIMONIC/Twitter-Clone-With-Fsharp/blob/main/img/Picture1.png)

Fig 1. System architecture of the twitter clone system.

## How to **Run**

### 1. Twitter engine

```
dotnet fsi  server.fsx
dotnet fsi  client.fsx <IP_Address> <Port>
```

- IP_Address: input the client’s ip address
- Port: input the client’s port

#### For simulation:

```
dotnet fsi tester.fsx
```

### 2. WebSharper

#### **Requirements**

```bash
dotnet5.0
netcoreapp3.1
```

#### Scripts

```bash
dotnet restore
dotnet build
```
