namespace DbVendo.Client

module internal Request =

#if FABLE_JS

    open Fable.Core
    open Fable.Core.JsInterop
    open Fetch

    importSideEffects "isomorphic-fetch"

    type Data =
        | Post of (string * string)
        | Get of string

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        let fromStringtoJsonBody (value: string) : BodyInit = value |> (!^)

        member __.Dispose() = ()

        member __.SendAsync (url: string) (xCorrelationID: string) (accept: string) (data: Data) : Async<string> =

            async {
                log "url: " url

                let url =
                    match data with
                    | Post _ -> url
                    | Get path -> url + (System.Uri.EscapeDataString path)

                let properties =
                    match data with
                    | Post(contentType, body) ->
                        log "body: " body

                        [ RequestProperties.Method HttpMethod.POST
                          requestHeaders
                              [ ContentType contentType
                                AcceptEncoding "gzip, br, deflate"
                                Accept accept
                                UserAgent "db-vendo-client"
                                Custom("X-Correlation-ID", xCorrelationID) ]
                          RequestProperties.Body(fromStringtoJsonBody body) ]
                    | Get path ->
                        [ RequestProperties.Method HttpMethod.GET
                          requestHeaders
                              [ AcceptEncoding "gzip, br, deflate"
                                Accept accept
                                UserAgent "db-vendo-client"
                                Custom("X-Correlation-ID", xCorrelationID) ] ]

                let! response =
                    fetch url properties
                    |> Promise.catch (fun ex ->
                        printfn "sendAsync: %s" ex.Message
                        raise ex)
                    |> Async.AwaitPromise

                let! text = response.text () |> Async.AwaitPromise
                log "response:" text

                return
                    match response.Ok with
                    | true -> text
                    | false -> failwith text
            }

#else

#if FABLE_PY

    open System.Collections.Generic

    open Fable.Core
    open Fable.Core.JsInterop

    [<Import("dumps", from = "json")>]
    [<Emit("dumps($1)")>]
    let private dumps (json: obj) : string = jsNative

    [<Import("post", from = "requests")>]
    [<Emit("post($1, data=$2.encode('utf-8'), headers=$3)")>]
    let private post (url: string) (body: string) (headers: Dictionary<string, string>) : obj = jsNative

    [<Import("get", from = "requests")>]
    [<Emit("get($1, headers=$2)")>]
    let private get (url: string) (headers: Dictionary<string, string>) : obj = jsNative

    [<Emit("$0.status_code")>]
    let private statusCode (r: obj) : int = jsNative

    [<Emit("$0.text")>]
    let private text (r: obj) : string = jsNative

    [<Emit("$0.json()")>]
    let private toJson (r: obj) : string = jsNative

    type Data =
        | Post of (string * string)
        | Get of string

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        member __.Dispose() = ()

        member __.SendAsync (url: string) (xCorrelationID: string) (accept: string) (data: Data) : Async<string> =

            log "url: " url

            let headers = Dictionary<_, _>()

            headers.Add("Accept-Encoding", "gzip, br, deflate")
            headers.Add("Accept", accept)
            headers.Add("User-Agent", "db-vendo-client")
            headers.Add("X-Correlation-ID", xCorrelationID)

            async {

                let r =
                    match data with
                    | Post(contentType, body) ->
                        headers.Add("Content-Type", contentType)
                        log "body: " body
                        post url body headers
                    | Get path -> get (url + (System.Uri.EscapeDataString path)) headers

                if (statusCode r) = 200 then
                    let txt = (dumps (toJson r))
                    log "response:" txt
                    return txt
                else
                    log "statusCode: " (statusCode r)
                    let txt = text r
                    log "text: " txt
                    return failwith txt
            }

#else

    open System.Net

    type Data =
        | Post of (string * string)
        | Get of string

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        let createHandler () =
            let handler = new Http.HttpClientHandler()
            handler.AutomaticDecompression <- DecompressionMethods.GZip
            handler.AllowAutoRedirect <- true
            handler

        let client = new Http.HttpClient(createHandler ())

        member __.Dispose() = client.Dispose()

        member __.SendAsync (url: string) (xCorrelationID: string) (accept: string) (data: Data) : Async<string> =
            async {
                log "url: " url

                use req =
                    match data with
                    | Post(contentType, body) ->
                        log "body: " body
                        let req = new Http.HttpRequestMessage(Http.HttpMethod.Post, url)
                        req.Content <- new Http.StringContent(body, Http.Headers.MediaTypeHeaderValue(contentType))
                        req
                    | Get path ->
                        let req =
                            new Http.HttpRequestMessage(
                                Http.HttpMethod.Get,
                                url + (System.Web.HttpUtility.UrlEncode path)
                            )

                        req

                req.Headers.Add("Accept-Encoding", "gzip, br, deflate")
                req.Headers.Add("Accept-Language", "en")
                req.Headers.Add("user-agent", "db-vendo-client")
                req.Headers.Add("X-Correlation-ID", xCorrelationID)
                req.Headers.Add("Accept", accept)

                let! response = client.SendAsync(req) |> Async.AwaitTask
                let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

                log "response:" body

                return
                    match response.IsSuccessStatusCode with
                    | true -> body
                    | false -> failwith body
            }

#endif
#endif
