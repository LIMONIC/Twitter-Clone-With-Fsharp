(function(Global)
{
 "use strict";
 var WebSharper,Client,Site,TwitterClone_Templates,console,UI,Var$1,Templating,Runtime,Server,ProviderBuilder,Handler,TemplateInstance,Doc,AttrProxy,Client$1,Templates,Concurrency,Remoting,AjaxRemotingProvider,IntelliFactory,Runtime$1;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 Client=WebSharper.Client=WebSharper.Client||{};
 Site=WebSharper.Site=WebSharper.Site||{};
 TwitterClone_Templates=Global.TwitterClone_Templates=Global.TwitterClone_Templates||{};
 console=Global.console;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 Templating=UI&&UI.Templating;
 Runtime=Templating&&Templating.Runtime;
 Server=Runtime&&Runtime.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 IntelliFactory=Global.IntelliFactory;
 Runtime$1=IntelliFactory&&IntelliFactory.Runtime;
 Client.Login$75$21=function()
 {
  return function(e)
  {
   console.log(e.Vars.Hole("inputusername").$1.Get()+" : "+e.Vars.Hole("inputuserpass").$1.Get());
  };
 };
 Client.Login=function()
 {
  var b,t,p,i;
  Var$1.Create$1("");
  Var$1.Create$1("");
  return(b=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"onlogin",function()
  {
   return t.i;
  },function(e)
  {
   console.log(e.Vars.Hole("inputusername").$1.Get()+" : "+e.Vars.Hole("inputuserpass").$1.Get());
  })),t)),(p=Handler.CompleteHoles(b.k,b.h,[["inputusername",0],["inputuserpass",0]]),(i=new TemplateInstance.New(p[1],TwitterClone_Templates.loginblock(p[0])),b.i=i,i))).get_Doc();
 };
 Client.Test=function()
 {
  var inputField,copyTheInput;
  inputField=Doc.Input([],Var$1.Create$1(""));
  copyTheInput=Doc.Element("div",[AttrProxy.Create("class","panel-default")],[Doc.Element("div",[AttrProxy.Create("class","panel-body")],[inputField])]);
  Templates.LoadLocalTemplates("");
  Doc.RunById("main",copyTheInput);
 };
 Client.Db$40$22=function()
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
 Client.Main$25$22=function()
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
 Client.Main$17$20=function(rvReversed)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoSomething:-2030603568",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
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
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("TwitterClone:WebSharper.Server.DoSomething:-2030603568",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
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
 Site.Twitter$87$34=Runtime$1.Curried3(function($1,$2,$3)
 {
  self.document.getElementById("userId");
  Global.alert("Alert");
  return Client.Test();
 });
 Site.Twitter$78$38=Runtime$1.Curried3(function($1,$2,$3)
 {
  return Global.alert("userId");
 });
 TwitterClone_Templates.loginblock=function(h)
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
