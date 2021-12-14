(function(Global)
{
 "use strict";
 var WebSharper,Client,Site,TwitterClone_Templates,IntelliFactory,Runtime,Utils,console,Concurrency,Remoting,AjaxRemotingProvider,UI,Var$1,Templating,Runtime$1,Server,ProviderBuilder,Handler,TemplateInstance,Doc,AttrProxy,Client$1,Templates;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 Client=WebSharper.Client=WebSharper.Client||{};
 Site=WebSharper.Site=WebSharper.Site||{};
 TwitterClone_Templates=Global.TwitterClone_Templates=Global.TwitterClone_Templates||{};
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 Utils=WebSharper&&WebSharper.Utils;
 console=Global.console;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 Templating=UI&&UI.Templating;
 Runtime$1=Templating&&Templating.Runtime;
 Server=Runtime$1&&Runtime$1.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 Client.Register$93$24=function(resJsonStr)
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
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoRegister:-527084609",[userId,userPass,prop]),function(a)
    {
     resJsonStr.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Register=function()
 {
  var resJsonStr,b,R,_this,t,p,i;
  resJsonStr=Var$1.Create$1("");
  return(b=(R=resJsonStr.get_View(),(_this=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"onregister",function()
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
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoRegister:-527084609",[userId,userPass,prop]),function(a)
    {
     resJsonStr.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  })),t)),(_this.h.push({
   $:2,
   $0:"result",
   $1:R
  }),_this))),(p=Handler.CompleteHoles(b.k,b.h,[["registerusername",0],["registeruseremail",0],["registeruserpass",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.loginblock(p[0])),b.i=i,i))).get_Doc();
 };
 Client.Login$78$21=function(resJsonStr)
 {
  return function(e)
  {
   var userId,userPass,b;
   userId=e.Vars.Hole("inputusername").$1.Get();
   userPass=e.Vars.Hole("inputuserpass").$1.Get();
   console.log(userId+" : "+userPass);
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogin:446134978",[userId,userPass]),function(a)
    {
     resJsonStr.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Login=function()
 {
  var resJsonStr,b,R,_this,t,p,i;
  resJsonStr=Var$1.Create$1("");
  return(b=(R=resJsonStr.get_View(),(_this=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"onlogin",function()
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
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoLogin:446134978",[userId,userPass]),function(a)
    {
     resJsonStr.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  })),t)),(_this.h.push({
   $:2,
   $0:"result",
   $1:R
  }),_this))),(p=Handler.CompleteHoles(b.k,b.h,[["inputusername",0],["inputuserpass",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.loginblock$1(p[0])),b.i=i,i))).get_Doc();
 };
 Client.Test=function()
 {
  var inputField,copyTheInput;
  inputField=Doc.Input([],Var$1.Create$1(""));
  copyTheInput=Doc.Element("div",[AttrProxy.Create("class","panel-default")],[Doc.Element("div",[AttrProxy.Create("class","panel-body")],[inputField])]);
  Templates.LoadLocalTemplates("");
  Doc.RunById("main",copyTheInput);
 };
 Client.Db$44$22=function()
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
 Client.Main$29$22=function()
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
 Client.Main$21$20=function(rvReversed)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoSomething:-1277680967",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
    {
     rvReversed.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Main=function()
 {
  var rvReversed,b,_this,t,R,_this$1,t$1,p,i;
  rvReversed=Var$1.Create$1("");
  return(b=(_this=(t=(R=rvReversed.get_View(),(_this$1=(t$1=new ProviderBuilder.New$1(),(t$1.h.push(Handler.EventQ2(t$1.k,"onsend",function()
  {
   return t$1.i;
  },function(e)
  {
   var b$1;
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoSomething:-1277680967",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
    {
     rvReversed.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  })),t$1)),(_this$1.h.push({
   $:2,
   $0:"reversed",
   $1:R
  }),_this$1))),(t.h.push(Handler.EventQ2(t.k,"oninitdb",function()
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
 Site.Twitter$99$34=Runtime.Curried3(function($1,$2,$3)
 {
  self.document.getElementById("userId");
  Global.alert("Alert");
  return Client.Test();
 });
 Site.Twitter$90$38=Runtime.Curried3(function($1,$2,$3)
 {
  return Global.alert("userId");
 });
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
}(self));
