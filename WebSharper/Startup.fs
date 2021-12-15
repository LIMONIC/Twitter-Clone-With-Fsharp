namespace WebSharper

open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open WebSharper.AspNetCore
open WebSharper.AspNetCore.WebSocket

type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddSitelet(Site.Main)
            .AddAuthentication("WebSharper")
            .AddCookie("WebSharper", fun options -> ())
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

        app.UseAuthentication()
            .UseStaticFiles()
            .UseWebSockets()
            .UseWebSharper(
                fun builder ->
                    builder.UseWebSocket(
                        "ws",
                        fun wsBuilder ->
                            wsBuilder
                                .Use(WebSocketServiceProvider.Start())
                                .JsonEncoding(JsonEncoding.Readable) |> ignore
                )
            )
            .Run(fun context ->
                context.Response.StatusCode <- 404
                context.Response.WriteAsync("Page not found"))

module Program =
    let BuildWebHost args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .Build()

    [<EntryPoint>]
    let main args =
        BuildWebHost(args).Run()
        0
