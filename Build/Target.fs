module Target

open System.IO

open Command

let HOME = System.Environment.GetEnvironmentVariable "HOME"

let buildDotnet () =
    dotnet "build" [ "src/fshafas.api/target.dotnet/fshafas.api.fsproj" ]
    dotnet "build" [ "src/dbvendo/target.dotnet/dbvendo.fsproj" ]
    dotnet "build" [ "src/fshafas/target.dotnet/fshafas.fsproj" ]
    dotnet "build" [ "src/fshafas.profiles/target.dotnet/fshafas.profiles.fsproj" ]

let testDotnet () =
    dotnet "test" [ "--verbosity"; "q"; "src/fshafas.test/fshafastest.fsproj" ]

let buildNupkg (version: string) (srcDir: string) (fsproj: string) (nugetDir: string) (nugetPkg: string) =
    let toDir = HOME + "/local.packages"

    if not (Directory.Exists toDir) then
        Directory.CreateDirectory(toDir) |> ignore

    dotnet "pack" [ "-c"; "Debug"; "/p:Version=" + version; (srcDir + "/" + fsproj) ]

    delete (HOME + "/local.packages/" + nugetDir + "/" + version)

    let nupkgFile = srcDir + "/bin/Debug/" + nugetPkg + "." + version + ".nupkg"

    nuget "add" [ nupkgFile; "-source"; toDir; "-expand" ]

    delete (HOME + "/.nuget/packages/" + nugetDir)

let buildDotnetNupkg (version: string) =
    buildNupkg version "src/fshafas.api/target.dotnet" "fshafas.api.fsproj" "fshafas.api" "FsHafas.Api"

    buildNupkg version "src/fshafas/target.dotnet" "fshafas.fsproj" "fshafas" "FsHafas"

    buildNupkg version "src/dbvendo/target.dotnet" "dbvendo.fsproj" "dbvendo" "DbVendo"

    buildNupkg
        version
        "src/fshafas.profiles/target.dotnet"
        "fshafas.profiles.fsproj"
        "fshafas.profiles"
        "FsHafas.Profiles"

let buildJavascriptNupkg (version: string) =
    buildNupkg
        version
        "src/fshafas/target.javascript"
        "fshafas.fable.javascript.fsproj"
        "fshafas.javascript"
        "FsHafas.JavaScript"

    buildNupkg
        version
        "src/fshafas.profiles/target.javascript"
        "fshafas.profiles.fable.javascript.fsproj"
        "fshafas.profiles.javascript"
        "FsHafas.Profiles.JavaScript"

    buildNupkg
        version
        "src/fshafas.api/target.javascript"
        "fshafas.api.fable.javascript.fsproj"
        "fshafas.api.javascript"
        "FsHafas.Api.JavaScript"

    buildNupkg
        version
        "src/dbvendo/target.javascript"
        "dbvendo.fable.javascript.fsproj"
        "dbvendo.javascript"
        "DbVendo.JavaScript"

let buildFableBindingNupkg (version: string) =
    buildNupkg
        version
        "src/hafas.client.bindings"
        "hafas.client.bindings.fsproj"
        "hafas.client.bindings"
        "Hafas.Client.Bindings"

let checkVersionInFile (version: string) (file: string) =
    match
        File.ReadAllLines file
        |> Array.exists (fun line -> line.Contains("\"version\": \"" + version))
    with
    | true -> ()
    | false -> raise (System.ArgumentException($"version {version} not found in file {file}"))

let buildJavascript (version: string) =
    checkVersionInFile version "src/fshafas.javascript.package/fs-hafas-client/package.json"

    checkVersionInFile version "src/fshafas.profiles.javascript.package/fs-hafas-profiles/package.json"

    delete "./src/fshafas.javascript.package/fs-hafas-client/fable_modules"

    dotnet
        "fable"
        [ "./src/fshafas.javascript.package/fshafas.fable.fsproj"
          "--typedArrays"
          "false"
          "--noCache"
          "--outDir"
          "./src/fshafas.javascript.package/fs-hafas-client" ]

    delete "./src/fshafas.profiles.javascript.package/fs-hafas-profiles/fable_modules"

    dotnet
        "fable"
        [ "./src/fshafas.profiles.javascript.package/fshafas.fable.fsproj"
          "--typedArrays"
          "false"
          "--noCache"
          "--outDir"
          "./src/fshafas.profiles.javascript.package/fs-hafas-profiles" ]

    delete "./src/fshafas.javascript.package/fs-hafas-client/fable_modules/.gitignore"

    npm "pack" [ "./src/fshafas.javascript.package/fs-hafas-client/" ]

    mv ("fs-hafas-client-" + version + ".tgz") "./src/fshafas.javascript.package/"

    delete "./src/fshafas.profiles.javascript.package/fs-hafas-profiles/fable_modules/.gitignore"

    npm "pack" [ "./src/fshafas.profiles.javascript.package/fs-hafas-profiles/" ]

    mv ("fs-hafas-profiles-" + version + ".tgz") "./src/fshafas.profiles.javascript.package/"

let buildJavascriptTest () =
    dotnet "build" [ "src/examples/cli/target.dotnet/cli.fsproj" ]

    dotnet
        "fable"
        [ "src/examples/cli/target.javascript/cli.fable.javascript.fsproj"
          "--outDir"
          "src/examples/cli/target.javascript/build/"
          "--noCache" ]

    dotnet
        "test"
        [ "src/fshafas.package.test/fshafaspackagetest.fsproj"
          "--filter"
          "DotnetEqualsToJavaScript" ]

let buildPythonNupkg (version: string) =
    buildNupkg
        version
        "src/fshafas.api/target.python"
        "fshafas.api.fable.python.fsproj"
        "fshafas.api.python"
        "FsHafas.Api.Python"

    buildNupkg version "src/dbvendo/target.python" "dbvendo.fable.python.fsproj" "dbvendo.python" "DbVendo.Python"

    buildNupkg version "src/fshafas/target.python" "fshafas.fable.python.fsproj" "fshafas.python" "FsHafas.Python"

    buildNupkg
        version
        "src/fshafas.profiles/target.python"
        "fshafas.profiles.fable.python.fsproj"
        "fshafas.profiles.python"
        "FsHafas.Profiles.Python"

let checkPythonVersionInFile (version: string) (file: string) =
    let splits = version.Split [| '-' |]

    let version = if splits.Length = 3 then splits.[2] else version

    match
        File.ReadAllLines file
        |> Array.exists (fun line -> line.Contains("version='" + version + "'"))
    with
    | true -> ()
    | false -> raise (System.ArgumentException($"version {version} not found in file {file}"))

let buildPython (version: string) =
    checkPythonVersionInFile version "src/fshafas.python.package/setup.py"

    dotnet
        "fable"
        [ "src/fshafas.python.package/fshafas/fshafas.fsproj"
          "--lang"
          "Python"
          "--noCache" ]

    exec "./fixes.sh" "src/fshafas.python.package/"

    python "build" [ "src/fshafas.python.package/" ]

let buildPythonTest () =
    dotnet "build" [ "src/examples/cli/target.dotnet/cli.fsproj" ]

    dotnet
        "fable"
        [ "src/examples/cli/target.javascript/cli.fable.javascript.fsproj"
          "--lang"
          "Python"
          "--noCache" ]

    dotnet
        "test"
        [ "src/fshafas.package.test/fshafaspackagetest.fsproj"
          "--filter"
          "DotnetEqualsToPython" ]
