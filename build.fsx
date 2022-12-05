#if FAKE
#r "paket: groupref Fake //"
#endif

#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.Tools
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open Fake.DotNet
open System
open System.Text.RegularExpressions

let release = ReleaseNotes.load "RELEASE_NOTES.md"
let releaseProfiles = ReleaseNotes.load "RELEASE_NOTES_Profiles.md"

let projectFiles = !! "src/*/*.*proj"

type Config = { version: string }

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/fshafas.fable/build"
    Shell.cleanDir "src/fshafas.javascript.package/build"
    Shell.rm "src/fshafas.javascript.package/fs-hafas-client/fshafas.bundle.js"
    Shell.rm "src/fshafas.javascript.package/fs-hafas-client/fshafas.bundle.js.map"

    projectFiles
    |> Seq.iter (fun x -> DotNet.exec id "clean" x |> ignore))

let checkResult (msg: string) (res: ProcessResult) = if not res.OK then failwith msg

let checkResultUnit (msg: string) (res: ProcessResult<unit>) = if res.ExitCode <> 0 then failwith msg

Target.create "BuildDocs" (fun _ ->
    Shell.cleanDir ".fsdocs"

    DotNet.exec id "build" "src/fshafas/fshafas.fsproj"
    |> checkResult "buildLib failed"

    DotNet.exec id "fsdocs" "build --clean --input src/fshafas/docs --output output/fshafas"
    |> ignore)

Target.create "ReleaseDocs" (fun _ ->
    Shell.cleanDir "gh-pages"
    let url = "https://github.com/bergmannjg/fshafas.git"
    Git.Repository.init "gh-pages" false false

    Git.Branches.checkout "gh-pages" true "gh-pages"

    Shell.copyRecursive "output/fshafas" "gh-pages" true
    |> printfn "%A"

    Git.Staging.stageAll "gh-pages"
    Git.Commit.exec "gh-pages" (sprintf "Update generated documentation %s" release.AssemblyVersion)

    CreateProcess.fromRawCommand "git" [ "push"; "--force"; url; "gh-pages" ]
    |> CreateProcess.withWorkingDirectory "gh-pages"
    |> Proc.run // start with the above configuration
    |> checkResultUnit "git push failed")

Target.create "BuildLib" (fun _ ->
    DotNet.exec id "build" "src/fshafas/target.dotnet/fshafas.fsproj"
    |> checkResult "buildLib failed"

    DotNet.exec id "build" "src/fshafas.profiles/target.dotnet/fshafas.profiles.fsproj"
    |> checkResult "buildLib failed")

Target.create "Test.FsHafas" (fun _ ->
    DotNet.exec id "test" "src/fshafas.test/fshafastest.fsproj"
    |> checkResult "Test failed")

Target.create "Test.JavaScript.Package" (fun _ ->
    DotNet.exec (DotNet.Options.withWorkingDirectory "src/examples/cli/target.dotnet") "build" "cli.fsproj"
    |> checkResult "Build failed"

    DotNet.exec
        (DotNet.Options.withWorkingDirectory "src/examples/cli/target.javascript")
        "fable"
        "cli.fable.javascript.fsproj -o build"
    |> checkResult "Build failed"

    DotNet.exec id "test" "src/fshafas.package.test/fshafaspackagetest.fsproj --filter DotnetEqualsToJavaScript"
    |> checkResult "Test failed")

Target.create "Test.Python.Package" (fun _ ->
    DotNet.exec (DotNet.Options.withWorkingDirectory "src/examples/cli/target.dotnet") "build" "cli.fsproj"
    |> checkResult "Build failed"

    DotNet.exec
        (DotNet.Options.withWorkingDirectory "src/examples/cli/target.python")
        "fable"
        "cli.fable.python.fsproj --lang Python"
    |> checkResult "Build failed"

    DotNet.exec id "test" "src/fshafas.package.test/fshafaspackagetest.fsproj --filter DotnetEqualsToPython"
    |> checkResult "Test failed")

let getVersion (s: string) =
    let rx = new Regex(@"""version"":\s*""([^""]+)")
    let m = rx.Match(s)

    if m.Success then
        m.Groups.[1].Value
    else
        ""

let CheckReleaseVersion (file: string) (exptectedVersion: string) =
    let version = getVersion (File.readAsString (file))

    if exptectedVersion <> version then
        raise (System.Exception(sprintf "config version, exptected: %s, actual: %s" exptectedVersion version))

Target.create "CheckReleaseVersion" (fun _ ->
    CheckReleaseVersion "src/fshafas.javascript.package/fs-hafas-client/package.json" release.AssemblyVersion

    CheckReleaseVersion
        "src/fshafas.profiles.javascript.package/fs-hafas-profiles/package.json"
        releaseProfiles.AssemblyVersion)

Target.create "BuildJavaScriptPackage" (fun _ ->
    Shell.cleanDir ("./src/fshafas.javascript.package/fs-hafas-client/fable_modules")

    let argsClient =
        "./fshafas.fable.fsproj --typedArrays false --noCache --outDir ./fs-hafas-client"

    DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.javascript.package") "fable" argsClient
    |> checkResult "BuldFableWebpack failed"

    let argsProfile =
        "./fshafas.fable.fsproj --typedArrays false --noCache --outDir ./fs-hafas-profiles"

    Shell.cleanDir ("./src/fshafas.profiles.javascript.package/build")

    DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.profiles.javascript.package") "fable" argsProfile
    |> checkResult "BuldFableWebpack failed")

Target.create "PackJavaScriptPackage" (fun _ ->
    Npm.exec "pack fs-hafas-client/" (fun o ->
        { o with
            NpmFilePath = "/usr/bin/npm"
            WorkingDirectory = "./src/fshafas.javascript.package/" })
    |> ignore

    Npm.exec "pack fs-hafas-profiles/" (fun o ->
        { o with
            NpmFilePath = "/usr/bin/npm"
            WorkingDirectory = "./src/fshafas.profiles.javascript.package/" })
    |> ignore)

let run workingDirectory file arguments =
    use __ = Trace.traceTask file ""

    let startInfo =
        new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)

    startInfo.WorkingDirectory <- workingDirectory

    let callResult =
        startInfo
        |> CreateProcess.ofStartInfo
        |> CreateProcess.redirectOutput
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
        |> Proc.run

    if callResult.ExitCode = 0 then
        Trace.trace callResult.Result.Output
    else
        Trace.traceError callResult.Result.Error
        failwith (file + " encountered errors!")

let runWithFailureResult workingDirectory file arguments =
    use __ = Trace.traceTask file ""

    let startInfo =
        new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)

    startInfo.WorkingDirectory <- workingDirectory

    let callResult =
        startInfo
        |> CreateProcess.ofStartInfo
        |> CreateProcess.redirectOutput
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
        |> Proc.run

    if callResult.ExitCode = 0 then
        Trace.trace callResult.Result.Output
    else
        Trace.traceError callResult.Result.Output

    callResult.ExitCode = 0

let runWithResult workingDirectory file arguments =
    use __ = Trace.traceTask file ""

    let startInfo =
        new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)

    startInfo.WorkingDirectory <- workingDirectory

    let callResult =
        startInfo
        |> CreateProcess.ofStartInfo
        |> CreateProcess.redirectOutput
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
        |> Proc.run

    if callResult.ExitCode = 0 then
        callResult.Result.Output
    else
        Trace.traceError callResult.Result.Output
        callResult.Result.Error

let CreateFableFsProjFile (srcDir: string) (file: string) (fsproj: string) =
    let items =
        System.IO.File.ReadAllLines file
        |> Array.filter (fun line -> line.Contains "Compile")
        |> Array.map (fun line -> line.Replace("../", ""))

    let defineConstants =
        System.IO.File.ReadAllLines file
        |> Array.filter (fun line -> line.Contains "DefineConstants")
        |> (fun lines ->
            if lines.Length = 1 then
                lines.[0]
            else
                "    <DefineConstants>FABLE_COMPILER</DefineConstants>")

    let content =
        $"""<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>
  <PropertyGroup>
{defineConstants}
  </PropertyGroup>
  <ItemGroup>
{String.concat "\n" items}
  </ItemGroup>
</Project>"""

    System.IO.File.WriteAllText(srcDir + "/nuget/" + fsproj, content)

let PublishToLocalNuGetFeed (version: string) (srcDir: string) (fsproj: string) (nugetDir: string) (nugetPkg: string) =
    let versionParameter = "/p:Version=\"" + version + "\""

    if srcDir.Contains "profiles" then
        CreateFableFsProjFile srcDir (srcDir + "/../files.props") "fshafas.profiles.fsproj"
    else
        CreateFableFsProjFile srcDir (srcDir + "/" + fsproj) "fshafas.fsproj"

    DotNet.exec id "pack" (versionParameter + " " + srcDir + "/" + fsproj)
    |> checkResult "pack failed"

    let home = Environment.environVar "HOME"

    Shell.cleanDir (
        home
        + "/local.packages/"
        + nugetDir
        + "/"
        + version
    )

    run
        ("./" + srcDir + "/")
        "/usr/local/bin/nuget.exe"
        ("add"
         + " bin/Debug/"
         + nugetPkg
         + "."
         + version
         + ".nupkg"
         + " -source "
         + home
         + "/local.packages -expand")

    Shell.cleanDir (
        home
        + "/.nuget/packages/"
        + nugetDir
        + "/"
        + version
    )

    DotNet.exec id "paket" ("update " + nugetDir)
    |> ignore

Target.create "PublishJavaScriptToLocalNuGetFeed" (fun _ ->
    PublishToLocalNuGetFeed
        release.AssemblyVersion
        "src/fshafas/target.javascript"
        "fshafas.fable.javascript.fsproj"
        "fshafas.javascript"
        "FsHafas.JavaScript"

    PublishToLocalNuGetFeed
        releaseProfiles.AssemblyVersion
        "src/fshafas.profiles/target.javascript"
        "fshafas.profiles.fable.javascript.fsproj"
        "fshafas.profiles.javascript"
        "FsHafas.Profiles.JavaScript")

let getPyVersion (file: string) =
    let setup = File.readAsString file
    let versionRegex = System.Text.RegularExpressions.Regex @"version='([0-9.]*)'"

    let m = versionRegex.Match setup

    if m.Groups.Count > 0 then
        m.Groups.[1].Value
    else
        ""

Target.create "PublishPythonToLocalNuGetFeed" (fun _ ->
    let versionSuffix = getPyVersion "src/fshafas.python.package/setup.py"

    let version = release.AssemblyVersion + "-beta-" + versionSuffix

    PublishToLocalNuGetFeed
        version
        "src/fshafas/target.python"
        "fshafas.fable.python.fsproj"
        "fshafas.python"
        "FsHafas.Python"

    PublishToLocalNuGetFeed
        version
        "src/fshafas.profiles/target.python"
        "fshafas.profiles.fable.python.fsproj"
        "fshafas.profiles.python"
        "FsHafas.Profiles.Python")

Target.create "InstallPythonPackage" (fun _ ->
    let projDir = "src/fshafas.python.package/"

    Shell.cleanDir (projDir + "fshafas.egg-info/")

    Shell.Exec("python3", "-m pip uninstall -y fshafas")
    |> ignore

    let version = getPyVersion "src/fshafas.python.package/setup.py"
    let wheel = "dist/fshafas-" + version + "-py3-none-any.whl"

    Shell.Exec("python3", "-m pip install " + projDir + wheel)
    |> ignore)

Target.create "BuildPythonPackage" (fun _ ->
    let projDir = "src/fshafas.python.package/"
    Shell.cleanDir (projDir + "fable_modules/")

    DotNet.exec id "fable" (projDir + "fshafas.fsproj" + " " + "--lang Python")
    |> ignore

    Shell.cleanDir (projDir + "fshafas/fable_modules/")
    Shell.deleteDir (projDir + "fshafas/fable_modules/")
    Shell.mv (projDir + "fable_modules/") (projDir + "fshafas")
    Shell.Exec("bash", "fixes.sh", projDir) |> ignore

    Shell.Exec("python3", "-m build " + projDir)
    |> ignore)

let sourceFiles =
    !! "src/**/*.fs" ++ "src/**/*.fsi" ++ "build.fsx"
    -- "src/**/obj/**/*.fs"
    -- "src/**/bin/**/*.fs"
    -- "src/**/fable_modules/**/*.fs"

Target.create "Format" (fun _ ->
    let result =
        sourceFiles
        |> Seq.map (sprintf "\"%s\"")
        |> String.concat " "
        |> DotNet.exec id "fantomas"

    if not result.OK then
        printfn "Errors while formatting all files: %A" result.Messages)

Target.create "CheckFormat" (fun _ ->
    let result =
        sourceFiles
        |> Seq.map (sprintf "\"%s\"")
        |> String.concat " "
        |> sprintf "%s --check"
        |> DotNet.exec id "fantomas"

    if result.ExitCode = 0 then
        Trace.log "No files need formatting"
    elif result.ExitCode = 99 then
        failwith "Some files need formatting, run `dotnet fake build -t Format` to format them"
    else
        Trace.logf "Errors while formatting: %A" result.Errors
        failwith "Unknown errors while formatting")

open Fake.Core.TargetOperators

Target.create "Default" ignore

Target.create "Debug" ignore

Target.create "JavaScript" ignore

Target.create "Python" ignore

Target.create "Test.JavaScript" ignore

Target.create "Test.Python" ignore

Target.create "Docs" ignore

Target.create "Test" ignore

"BuildLib"
==> "Test.FsHafas"
==> "CheckFormat"
==> "Default"

"BuildLib"
==> "Test.FsHafas"
==> "CheckReleaseVersion"
==> "PublishJavaScriptToLocalNuGetFeed"
==> "BuildJavaScriptPackage"
==> "PackJavaScriptPackage"
==> "JavaScript"

"BuildLib"
==> "Test.FsHafas"
==> "PublishPythonToLocalNuGetFeed"
==> "BuildPythonPackage"
==> "InstallPythonPackage"
==> "Python"

"BuildLib"
==> "Test.FsHafas"
==> "PublishJavaScriptToLocalNuGetFeed"
==> "Test.JavaScript.Package"
==> "Test.JavaScript"

"BuildLib"
==> "Test.FsHafas"
==> "PublishPythonToLocalNuGetFeed"
==> "Test.Python.Package"
==> "Test.Python"

"BuildLib" ==> "BuildDocs" ==> "Docs"

Target.runOrDefault "Default"
