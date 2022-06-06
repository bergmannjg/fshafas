namespace FsHafas.Client

module internal Request =

#if FABLE_JS

    open Fable.Core
    open Fable.Core.JsInterop
    open Fetch

    importSideEffects "isomorphic-fetch"

    /// get window location if runnimg in a brower, excluding Node.Js and React Native
    [<Emit("typeof window !== 'undefined' && navigator.product !== 'ReactNative' ? (new URL(window.location)).origin : ''")>]
    let jsWindowLocation () : string = jsNative

    [<ImportDefault("md5")>]
    let md5 (x: byte []) : string = jsNative

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        let getMd5 (json: string) (salt: string) =

            let bytes = (System.Text.Encoding.UTF8.GetBytes(json + salt))

            md5 (bytes)

        let fromStringtoJsonBody (value: string) : BodyInit = value |> (!^)

        member __.Dispose() = ()

        member __.PostAsync (url: string) (salt: string) (json: string) =

            let urlchecksum =
                if salt.Length > 0 then
                    let checksum = getMd5 json salt
                    url + "?checksum=" + checksum
                else
                    url

            let urlEscaped =
                let windowLocation = jsWindowLocation ()

                if windowLocation.Length > 0 then
                    windowLocation
                    + "/proxy?url="
                    + System.Uri.EscapeDataString(urlchecksum)
                else
                    urlchecksum

            log "url: " urlEscaped

            let properties =
                [ RequestProperties.Method HttpMethod.POST
                  requestHeaders [ ContentType "application/json"
                                   AcceptEncoding "gzip, br, deflate"
                                   Accept "application/json"
                                   UserAgent "agent" ]
                  RequestProperties.Body(fromStringtoJsonBody json) ]

            fetch urlEscaped properties
            |> Promise.bind (fun res ->
                if not res.Ok then
                    raise (System.Exception(res.StatusText))

                res.text ())
            |> Promise.catch (fun ex ->
                printfn "PostAsync: %s" ex.Message
                raise ex)
            |> Async.AwaitPromise

#else

#if FABLE_PY

    open System.Collections.Generic

    open Fable.Core
    open Fable.Core.JsInterop

    [<ImportMember("hashlib")>]
    let private md5 (x: byte []) : obj = jsNative

    [<Emit("$0.hexdigest()")>]
    let private hexdigest (_: obj) : string = jsNative

    [<Emit("$0.encode()")>]
    let private toBytes (_: string) : byte [] = jsNative

    [<Import("dumps", from = "json")>]
    [<Emit("dumps($1)")>]
    let private dumps (json: obj) : string = jsNative

    [<Import("post", from = "requests")>]
    [<Emit("post($1, data=$2.encode('utf-8'), headers=$3)")>]
    let private post (url: string) (body: string) (headers: Dictionary<string, string>) : obj = jsNative

    [<Emit("$0.status_code")>]
    let private statusCode (r: obj) : int = jsNative

    [<Emit("$0.text")>]
    let private text (r: obj) : string = jsNative

    [<Emit("$0.json()")>]
    let private toJson (r: obj) : string = jsNative

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        let getMd5 (json: string) (salt: string) = hexdigest (md5 (toBytes (json + salt)))

        member __.Dispose() = ()

        member __.PostAsync (url: string) (salt: string) (json: string) =

            let urlchecksum =
                if salt.Length > 0 then
                    let checksum = getMd5 json salt
                    url + "?checksum=" + checksum
                else
                    url

            let urlEscaped = urlchecksum

            log "url: " urlEscaped

            let headers = Dictionary<_, _>()
            headers.Add("Content-Type", "application/json; charset=utf-8")
            headers.Add("Accept-Encoding", "gzip, br, deflate")
            headers.Add("Accept", "application/json")
            headers.Add("User-Agent", "agent")

            async {

                let r = post urlEscaped json headers

                if (statusCode r) = 200 then
                    return (dumps (toJson r))
                else
                    log "statusCode: " (statusCode r)
                    log "text: " (text r)
                    return ""
            }

#else

    open System.Security.Cryptography
    open System.Text
    open System.Net

    type HttpClient() =

        let log msg o = FsHafas.Client.Log.Print msg o

        let createHandler () =
            let handler = new Http.HttpClientHandler()
            handler.AutomaticDecompression <- DecompressionMethods.GZip
            handler

        let client = new Http.HttpClient(createHandler ())

        let md5 (data: byte array) : string =
            use md5 = MD5.Create()

            (StringBuilder(), md5.ComputeHash(data))
            ||> Array.fold (fun sb b -> sb.Append(b.ToString("x2")))
            |> string

        let getMd5 (json: string) (salt: string) =
            let bytes = System.Text.Encoding.UTF8.GetBytes(json + salt)

            let hash = md5 bytes

            hash

        member __.Dispose() = client.Dispose()

        member __.PostAsync (url: string) (salt: string) (json: string) =
            async {

                let urlchecksum =
                    if salt.Length > 0 then
                        let checksum = getMd5 json salt
                        url + "?checksum=" + checksum
                    else
                        url

                log "url: " urlchecksum

                use content = new Http.StringContent(json, Encoding.UTF8, "application/json")

                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, br, deflate")
                client.DefaultRequestHeaders.Add("Accept", "application/json")
                client.DefaultRequestHeaders.Add("user-agent", "agent")

                let! response =
                    client.PostAsync(urlchecksum, content)
                    |> Async.AwaitTask

                let! body =
                    response.Content.ReadAsStringAsync()
                    |> Async.AwaitTask

                return
                    match response.IsSuccessStatusCode with
                    | true -> body
                    | false -> body
            }

#endif
#endif
