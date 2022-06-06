open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open AspNetCore.Proxy
open AspNetCore.Proxy.Builders

/// corresponds to HttpClient.PostAsync in src/fshafas/lib/Request.fs
let addProxies (proxies: IProxiesBuilder) =
    proxies.Map(
        "/proxy",
        (fun proxy ->
            proxy.UseHttp (fun (builder: IHttpProxyBuilder) ->
                builder.WithEndpoint(fun context _ -> context.Request.Query.["url"].[0])
                |> ignore)
            |> ignore)
    )

let configureApp (app: IApplicationBuilder) =
    app
        .UseStaticFiles()
        .UseRouting()
        .UseProxies(fun proxies -> proxies |> addProxies |> ignore)
    |> ignore

let configureServices (services: IServiceCollection) = services.AddProxies() |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
                .UseUrls("http://localhost:5000")
            |> ignore)
        .Build()
        .Run()

    0
