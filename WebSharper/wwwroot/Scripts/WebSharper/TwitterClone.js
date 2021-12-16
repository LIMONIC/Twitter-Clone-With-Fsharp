(function(Global)
{
 "use strict";
 var WebSharper,Client,TweetPushProcess,Templating,TwitterClone_Templates,TwitterClone_JsonEncoder,TwitterClone_JsonDecoder,Strings,IntelliFactory,Runtime,Utils,console,Concurrency,Remoting,AjaxRemotingProvider,JSON,Arrays,UI,Var$1,JavaScript,Promise,AspNetCore,WebSocket,Client$1,WithEncoding,Templating$1,Runtime$1,Server,ProviderBuilder,Handler,TemplateInstance,List,Doc,AttrProxy,Client$2,Templates,ClientSideJson,Provider;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 Client=WebSharper.Client=WebSharper.Client||{};
 TweetPushProcess=WebSharper.TweetPushProcess=WebSharper.TweetPushProcess||{};
 Templating=WebSharper.Templating=WebSharper.Templating||{};
 TwitterClone_Templates=Global.TwitterClone_Templates=Global.TwitterClone_Templates||{};
 TwitterClone_JsonEncoder=Global.TwitterClone_JsonEncoder=Global.TwitterClone_JsonEncoder||{};
 TwitterClone_JsonDecoder=Global.TwitterClone_JsonDecoder=Global.TwitterClone_JsonDecoder||{};
 Strings=WebSharper&&WebSharper.Strings;
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Utils=WebSharper&&WebSharper.Utils;
 console=Global.console;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 JSON=Global.JSON;
 Arrays=WebSharper&&WebSharper.Arrays;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 JavaScript=WebSharper&&WebSharper.JavaScript;
 Promise=JavaScript&&JavaScript.Promise;
 AspNetCore=WebSharper&&WebSharper.AspNetCore;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client$1=WebSocket&&WebSocket.Client;
 WithEncoding=Client$1&&Client$1.WithEncoding;
 Templating$1=UI&&UI.Templating;
 Runtime$1=Templating$1&&Templating$1.Runtime;
 Server=Runtime$1&&Runtime$1.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 List=WebSharper&&WebSharper.List;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 Client$2=UI&&UI.Client;
 Templates=Client$2&&Client$2.Templates;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 Client.Twitter$269$23=function(resJsonStr,wsServiceProvider)
 {
  return function(e)
  {
   var tweetId,tags,mentions,prop,b;
   tweetId=e.Vars.Hole("tweetid").$1.Get();
   tags="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("retweettags").$1.Get(),[","],0))+"\"";
   mentions="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("retweetmentions").$1.Get(),[","],0))+"\"";
   prop=((((Runtime.Curried(function($1,$2,$3,$4)
   {
    return $1("{\"tweetId\": \""+Utils.toSafe($2)+"\", \"tag\": ["+Utils.toSafe($3)+"], \"mention\": ["+Utils.toSafe($4)+"]}");
   },4))(Global.id))(tweetId))(tags))(mentions);
   console.log(prop);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoReTweet:-1915423140",[prop]),function(a)
    {
     var contentArr,resObj;
     resJsonStr.Set(a);
     self.location.reload();
     resObj=JSON.parse(a);
     if(resObj.status==="success")
      {
       try
       {
        contentArr=resObj.content;
       }
       catch(m)
       {
        contentArr=[];
       }
       wsServiceProvider.$0.Post({
        $:0,
        $0:Arrays.get(contentArr,0).userId,
        $1:Arrays.get(contentArr,0).tweetId
       });
       return Concurrency.Zero();
      }
     else
      return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Twitter$247$21=function(resJsonStr,wsServiceProvider)
 {
  return function(e)
  {
   var content,tags,mentions,prop,b;
   content=e.Vars.Hole("tweetcontent").$1.Get();
   tags="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("tweettags").$1.Get(),[","],0))+"\"";
   mentions="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("tweetmentions").$1.Get(),[","],0))+"\"";
   prop=((((Runtime.Curried(function($1,$2,$3,$4)
   {
    return $1("{\"content\": \""+Utils.toSafe($2)+"\", \"tag\": ["+Utils.toSafe($3)+"], \"mention\": ["+Utils.toSafe($4)+"]}");
   },4))(Global.id))(content))(tags))(mentions);
   console.log(prop);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoTweet:-1915423140",[prop]),function(a)
    {
     var contentArr,resObj;
     resJsonStr.Set(a);
     self.location.reload();
     resObj=JSON.parse(a);
     if(resObj.status==="success")
      {
       try
       {
        contentArr=resObj.content;
       }
       catch(m)
       {
        contentArr=[];
       }
       wsServiceProvider.$0.Post({
        $:0,
        $0:Arrays.get(contentArr,0).userId,
        $1:Arrays.get(contentArr,0).tweetId
       });
       return Concurrency.Zero();
      }
     else
      return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Twitter=function(ep)
 {
  var resJsonStr,wsServiceProvider,b,b$1,t,t$1,p,i;
  resJsonStr=Var$1.Create$1("");
  wsServiceProvider=null;
  Promise.OfAsync((b=null,Concurrency.Delay(function()
  {
   return WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((TwitterClone_JsonEncoder.j())(a));
   },function(a)
   {
    return(TwitterClone_JsonDecoder.j())(JSON.parse(a));
   },ep,function()
   {
    var b$2;
    b$2=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$3;
       b$3=null;
       return Concurrency.Delay(function()
       {
        var $1,$2,$3,content,div,divCard,res;
        if(msg.$==0)
         {
          res=msg.$0.$0;
          console.log(res);
          try
          {
           content=JSON.parse(res).content;
          }
          catch(m)
          {
           content=[];
          }
          $3=(div=self.document.getElementById("tweetsDemoPanel"),divCard=self.document.createElement("div"),divCard.setAttribute("class","card"),divCard.innerHTML=function($4)
          {
           return $4("<div class=\"card\">\r\n                                                    <div class=\"card-body\">\r\n                                                        <h5 class=\"card-title\">"+Global.String(Arrays.get(content,0).userId)+"</h5>\r\n                                                        <p class=\"card-text\">"+Global.String(Arrays.get(content,0).text)+"</p>\r\n                                                        <ul class=\"list-group list-group-flush\">\r\n                                                          <li class=\"list-group-item fs-6 fw-light\">tweetId: "+Global.String(Arrays.get(content,0).tweetId)+"</li>\r\n                                                          <li class=\"list-group-item fs-6 fw-light\">Time: "+Global.String(Arrays.get(content,0).timestamp)+"</li>\r\n                                                        </ul>\r\n                                                    </div>\r\n                                                </div>");
          }(Global.id),div.prepend.apply(div,[divCard]),Concurrency.Zero());
          return Concurrency.Combine($3,Concurrency.Delay(function()
          {
           return Concurrency.Return(state+1);
          }));
         }
        else
         return msg.$==3?(console.log("WebSocket Connection Close"),Concurrency.Return(state)):msg.$==1?(console.log("WebSocket Connection Error"),Concurrency.Return(state)):(console.log("WebSocket Connection Open"),Concurrency.Return(state));
       });
      };
     }]);
    });
   });
  }))).then(function(x)
  {
   wsServiceProvider={
    $:1,
    $0:x
   };
  });
  return(b$1=(t=(t$1=new ProviderBuilder.New$1(),(t$1.h.push(Handler.EventQ2(t$1.k,"ontweet",function()
  {
   return t$1.i;
  },function(e)
  {
   var content,tags,mentions,prop,b$2;
   content=e.Vars.Hole("tweetcontent").$1.Get();
   tags="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("tweettags").$1.Get(),[","],0))+"\"";
   mentions="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("tweetmentions").$1.Get(),[","],0))+"\"";
   prop=((((Runtime.Curried(function($1,$2,$3,$4)
   {
    return $1("{\"content\": \""+Utils.toSafe($2)+"\", \"tag\": ["+Utils.toSafe($3)+"], \"mention\": ["+Utils.toSafe($4)+"]}");
   },4))(Global.id))(content))(tags))(mentions);
   console.log(prop);
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoTweet:-1915423140",[prop]),function(a)
    {
     var contentArr,resObj;
     resJsonStr.Set(a);
     self.location.reload();
     resObj=JSON.parse(a);
     if(resObj.status==="success")
      {
       try
       {
        contentArr=resObj.content;
       }
       catch(m)
       {
        contentArr=[];
       }
       wsServiceProvider.$0.Post({
        $:0,
        $0:Arrays.get(contentArr,0).userId,
        $1:Arrays.get(contentArr,0).tweetId
       });
       return Concurrency.Zero();
      }
     else
      return Concurrency.Zero();
    });
   })),null);
  })),t$1)),(t.h.push(Handler.EventQ2(t.k,"onretweet",function()
  {
   return t.i;
  },function(e)
  {
   var tweetId,tags,mentions,prop,b$2;
   tweetId=e.Vars.Hole("tweetid").$1.Get();
   tags="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("retweettags").$1.Get(),[","],0))+"\"";
   mentions="\""+Strings.Join("\",\"",Strings.SplitChars(e.Vars.Hole("retweetmentions").$1.Get(),[","],0))+"\"";
   prop=((((Runtime.Curried(function($1,$2,$3,$4)
   {
    return $1("{\"tweetId\": \""+Utils.toSafe($2)+"\", \"tag\": ["+Utils.toSafe($3)+"], \"mention\": ["+Utils.toSafe($4)+"]}");
   },4))(Global.id))(tweetId))(tags))(mentions);
   console.log(prop);
   Concurrency.StartImmediate((b$2=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoReTweet:-1915423140",[prop]),function(a)
    {
     var contentArr,resObj;
     resJsonStr.Set(a);
     self.location.reload();
     resObj=JSON.parse(a);
     if(resObj.status==="success")
      {
       try
       {
        contentArr=resObj.content;
       }
       catch(m)
       {
        contentArr=[];
       }
       wsServiceProvider.$0.Post({
        $:0,
        $0:Arrays.get(contentArr,0).userId,
        $1:Arrays.get(contentArr,0).tweetId
       });
       return Concurrency.Zero();
      }
     else
      return Concurrency.Zero();
    });
   })),null);
  })),t)),(p=Handler.CompleteHoles(b$1.k,b$1.h,[["tweetcontent",0],["tweettags",0],["tweetmentions",0],["tweetid",0],["retweettags",0],["retweetmentions",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.twitterform(p[0])),b$1.i=i,i))).get_Doc();
 };
 Client.Account$175$24=function()
 {
  return function(e)
  {
   var unfollowID,b;
   unfollowID=e.Vars.Hole("unfollowid").$1.Get();
   console.log(unfollowID);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoUnfollow:-1915423140",[unfollowID]),function(a)
    {
     console.log(a);
     self.location.reload();
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Account$164$22=function()
 {
  return function(e)
  {
   var followID,b;
   followID=e.Vars.Hole("followid").$1.Get();
   console.log(followID);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoFollow:-1915423140",[followID]),function(a)
    {
     console.log(a);
     self.location.reload();
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Account=function()
 {
  var b,t,t$1,p,i;
  Var$1.Create$1("");
  return(b=(t=(t$1=new ProviderBuilder.New$1(),(t$1.h.push(Handler.EventQ2(t$1.k,"onfollow",function()
  {
   return t$1.i;
  },function(e)
  {
   var followID,b$1;
   followID=e.Vars.Hole("followid").$1.Get();
   console.log(followID);
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoFollow:-1915423140",[followID]),function(a)
    {
     console.log(a);
     self.location.reload();
     return Concurrency.Zero();
    });
   })),null);
  })),t$1)),(t.h.push(Handler.EventQ2(t.k,"onunfollow",function()
  {
   return t.i;
  },function(e)
  {
   var unfollowID,b$1;
   unfollowID=e.Vars.Hole("unfollowid").$1.Get();
   console.log(unfollowID);
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoUnfollow:-1915423140",[unfollowID]),function(a)
    {
     console.log(a);
     self.location.reload();
     return Concurrency.Zero();
    });
   })),null);
  })),t)),(p=Handler.CompleteHoles(b.k,b.h,[["followid",0],["unfollowid",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.accountform(p[0])),b.i=i,i))).get_Doc();
 };
 Client.FollowList=function(userId)
 {
  return List.map(function(txt)
  {
   return Doc.Element("div",[],[Doc.Element("a",[AttrProxy.Create("class","list-group-item")],[Doc.TextNode(txt)])]);
  },(new AjaxRemotingProvider.New()).Sync("TwitterClone:WebSharper.Server.getFollowersList:-1216520703",[userId]));
 };
 Client.Register$130$24=function()
 {
  return function(e)
  {
   var userId,userPass,userEmail,prop,b;
   userId=e.Vars.Hole("registerusername").$1.Get();
   userPass=e.Vars.Hole("registeruserpass").$1.Get();
   userEmail=e.Vars.Hole("registeruseremail").$1.Get();
   prop=(((Runtime.Curried3(function($1,$2,$3)
   {
    return $1("{\"email\": \""+Utils.toSafe($2)+"\", \"nickName\": \""+Utils.toSafe($3)+"\"}");
   }))(Global.id))(userEmail))(userId);
   console.log(userId+" : "+userPass+" : "+userEmail);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoRegister:-1512493584",[userId,userPass,prop]),function(a)
    {
     return JSON.parse(a).status==="success"?(self.location.replace("/"),Concurrency.Zero()):Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Register=function()
 {
  var b,t,p,i;
  Var$1.Create$1("");
  return(b=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"onregister",function()
  {
   return t.i;
  },function(e)
  {
   var userId,userPass,userEmail,prop,b$1;
   userId=e.Vars.Hole("registerusername").$1.Get();
   userPass=e.Vars.Hole("registeruserpass").$1.Get();
   userEmail=e.Vars.Hole("registeruseremail").$1.Get();
   prop=(((Runtime.Curried3(function($1,$2,$3)
   {
    return $1("{\"email\": \""+Utils.toSafe($2)+"\", \"nickName\": \""+Utils.toSafe($3)+"\"}");
   }))(Global.id))(userEmail))(userId);
   console.log(userId+" : "+userPass+" : "+userEmail);
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoRegister:-1512493584",[userId,userPass,prop]),function(a)
    {
     return JSON.parse(a).status==="success"?(self.location.replace("/"),Concurrency.Zero()):Concurrency.Zero();
    });
   })),null);
  })),t)),(p=Handler.CompleteHoles(b.k,b.h,[["registerusername",0],["registeruseremail",0],["registeruserpass",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.loginblock(p[0])),b.i=i,i))).get_Doc();
 };
 Client.Login$110$21=function()
 {
  return function(e)
  {
   var userId,userPass,b;
   userId=e.Vars.Hole("inputusername").$1.Get();
   userPass=e.Vars.Hole("inputuserpass").$1.Get();
   console.log(userId+" : "+userPass);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogin:1719588972",[userId,userPass]),function(a)
    {
     var status;
     status=JSON.parse(a).status;
     console.log(status);
     return status==="success"?(self.location.replace("/"),Concurrency.Zero()):Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Login=function()
 {
  var b,t,p,i;
  return(b=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"onlogin",function()
  {
   return t.i;
  },function(e)
  {
   var userId,userPass,b$1;
   userId=e.Vars.Hole("inputusername").$1.Get();
   userPass=e.Vars.Hole("inputuserpass").$1.Get();
   console.log(userId+" : "+userPass);
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogin:1719588972",[userId,userPass]),function(a)
    {
     var status;
     status=JSON.parse(a).status;
     console.log(status);
     return status==="success"?(self.location.replace("/"),Concurrency.Zero()):Concurrency.Zero();
    });
   })),null);
  })),t)),(p=Handler.CompleteHoles(b.k,b.h,[["inputusername",0],["inputuserpass",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.loginblock$1(p[0])),b.i=i,i))).get_Doc();
 };
 Client.Test=function()
 {
  var inputField,copyTheInput;
  inputField=Doc.Input([],Var$1.Create$1(""));
  copyTheInput=Doc.Element("div",[AttrProxy.Create("class","panel-default")],[Doc.Element("div",[AttrProxy.Create("class","panel-body")],[inputField])]);
  Templates.LoadLocalTemplates("");
  Doc.RunById("main",copyTheInput);
 };
 Client.Db$76$22=function()
 {
  return function()
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    (new AjaxRemotingProvider.New()).Send("TwitterClone:WebSharper.Server.DoDBInit:6",[]);
    return Concurrency.Zero();
   })),null);
  };
 };
 Client.Db=function()
 {
  var b,_this,t,p,i;
  return(b=(_this=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"oninitdb",function()
  {
   return t.i;
  },function()
  {
   var b$1;
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    (new AjaxRemotingProvider.New()).Send("TwitterClone:WebSharper.Server.DoDBInit:6",[]);
    return Concurrency.Zero();
   })),null);
  })),t)),(_this.h.push({
   $:1,
   $0:"result",
   $1:"Sccess!"
  }),_this)),(p=Handler.CompleteHoles(b.k,b.h,[["texttoreverse",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.mainform(p[0])),b.i=i,i))).get_Doc();
 };
 Client.guest$44$26=Runtime.Curried3(function($1,$2,$3)
 {
  var b;
  return Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   console.log("login");
   self.location.replace("/welcome");
   return Concurrency.Return(null);
  })),null);
 });
 Client.guest=function()
 {
  return Doc.Element("div",[AttrProxy.Create("style","margin-bottom: 20px")],[Doc.Element("p",[AttrProxy.Create("style","display: inline; margin: 0 10px 15px 0")],[Doc.TextNode("click here to log in:")]),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary mybtn"),AttrProxy.HandlerImpl("click",function()
  {
   return function()
   {
    var b;
    return Concurrency.Start((b=null,Concurrency.Delay(function()
    {
     console.log("login");
     self.location.replace("/welcome");
     return Concurrency.Return(null);
    })),null);
   };
  })],[Doc.TextNode("login")])]);
 };
 Client.LoggedInUser$29$26=Runtime.Curried3(function($1,$2,$3)
 {
  var b;
  return Concurrency.Start((b=null,Concurrency.Delay(function()
  {
   return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogout:-741060419",[]),function()
   {
    self.location.replace("/");
    return Concurrency.Return(null);
   });
  })),null);
 });
 Client.LoggedInUser=function()
 {
  return Doc.Element("div",[AttrProxy.Create("style","margin-bottom: 20px")],[Doc.Element("p",[AttrProxy.Create("style","display: inline; margin: 0 10px 15px 0")],[Doc.TextNode("click here to log out:")]),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary mybtn"),AttrProxy.HandlerImpl("click",function()
  {
   return function()
   {
    var b;
    return Concurrency.Start((b=null,Concurrency.Delay(function()
    {
     return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogout:-741060419",[]),function()
     {
      self.location.replace("/");
      return Concurrency.Return(null);
     });
    })),null);
   };
  })],[Doc.TextNode("logout")])]);
 };
 TweetPushProcess.ProcessBinding=function(ep)
 {
  var wsServiceProvider,b;
  wsServiceProvider=null;
  Promise.OfAsync((b=null,Concurrency.Delay(function()
  {
   return WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((TwitterClone_JsonEncoder.j())(a));
   },function(a)
   {
    return(TwitterClone_JsonDecoder.j())(JSON.parse(a));
   },ep,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var res,resObj,div,li;
        return msg.$==0?Concurrency.Combine((res=msg.$0.$0,(console.log(res),resObj=JSON.parse(res),div=self.document.getElementById("tweetsDemoPanel"),li=self.document.createElement("li"),li.appendChild(self.document.createTextNode(resObj.text)),li.setAttribute("class","list-group-item"),div.appendChild(li),Concurrency.Zero())),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        })):msg.$==3?(console.log("WebSocket Connection Close"),Concurrency.Return(state)):msg.$==1?(console.log("WebSocket Connection Error"),Concurrency.Return(state)):(console.log("WebSocket Connection Open"),Concurrency.Return(state));
       });
      };
     }]);
    });
   });
  }))).then(function(x)
  {
   wsServiceProvider={
    $:1,
    $0:x
   };
  });
 };
 Templating.Twitter2$102$28=function()
 {
  return function(e)
  {
   var mention,dom;
   mention=e.Vars.Hole("mentionsearch").$1.Get();
   dom=(new AjaxRemotingProvider.New()).Sync("TwitterClone:WebSharper.Server.getTweetsListString:-1876397925",["mention",mention]);
   self.document.getElementById("TwitterList4").innerHTML=dom;
  };
 };
 Templating.Twitter2$93$24=function()
 {
  return function(e)
  {
   var tag,dom;
   tag=e.Vars.Hole("tabsearch").$1.Get();
   dom=(new AjaxRemotingProvider.New()).Sync("TwitterClone:WebSharper.Server.getTweetsListString:-1876397925",["tag",tag]);
   self.document.getElementById("TwitterList3").innerHTML=dom;
  };
 };
 TwitterClone_Templates.twitterform=function(h)
 {
  Templates.LoadLocalTemplates("twitter");
  return h?Templates.NamedTemplate("twitter",{
   $:1,
   $0:"twitterform"
  },h):void 0;
 };
 TwitterClone_Templates.accountform=function(h)
 {
  Templates.LoadLocalTemplates("account");
  return h?Templates.NamedTemplate("account",{
   $:1,
   $0:"accountform"
  },h):void 0;
 };
 TwitterClone_Templates.loginblock=function(h)
 {
  Templates.LoadLocalTemplates("register");
  return h?Templates.NamedTemplate("register",{
   $:1,
   $0:"loginblock"
  },h):void 0;
 };
 TwitterClone_Templates.loginblock$1=function(h)
 {
  Templates.LoadLocalTemplates("login");
  return h?Templates.NamedTemplate("login",{
   $:1,
   $0:"loginblock"
  },h):void 0;
 };
 TwitterClone_Templates.mainform=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",{
   $:1,
   $0:"mainform"
  },h):void 0;
 };
 TwitterClone_JsonEncoder.j=function()
 {
  return TwitterClone_JsonEncoder._v?TwitterClone_JsonEncoder._v:TwitterClone_JsonEncoder._v=(Provider.EncodeUnion(void 0,{
   tweetId:0
  },[["Info",[["$0","userId",Provider.Id(),0],["$1","tweetId",Provider.Id(),0]]]]))();
 };
 TwitterClone_JsonDecoder.j=function()
 {
  return TwitterClone_JsonDecoder._v?TwitterClone_JsonDecoder._v:TwitterClone_JsonDecoder._v=(Provider.DecodeUnion(void 0,"$",[[0,[["$0","Item",Provider.Id(),0]]]]))();
 };
}(self));
