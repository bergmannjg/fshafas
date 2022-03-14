#r "paket:
nuget FSharp.SystemTextJson
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MsBuild
nuget Fake.JavaScript.Npm
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
nuget Fake.Core.Xml
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.Tools
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open Fake.DotNet
open System
open System.Text.Json
open System.Text.Json.Serialization
open Fake.Core.Xml
open System.Text.RegularExpressions

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let projectFiles = !! "src/*/*.*proj"

type Config = { version: string }

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/fshafas.fable/build"
    Shell.cleanDir "src/fshafas.fable.package/build"
    Shell.rm "src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.js"
    Shell.rm "src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.js.map"
    projectFiles |> Seq.iter (fun x -> DotNet.exec id "clean" x |> ignore)
)

let checkResult (msg:string) (res: ProcessResult) =
  if not res.OK then failwith msg

let checkResultUnit (msg:string) (res: ProcessResult<unit>) =
  if res.ExitCode <> 0 then failwith msg

Target.create "BuildDocs" (fun _ ->
  DotNet.exec id "fsdocs" "build --clean --projects src/fshafas/fshafas.fsproj --input src/fshafas/docs/ --output output/fshafas" |> ignore
)

Target.create "ReleaseDocs" (fun _ ->
    Shell.cleanDir "gh-pages"
    let url = "https://github.com/bergmannjg/fshafas.git"
    Git.Repository.cloneSingleBranch "" url "gh-pages" "gh-pages"

    Git.Repository.fullclean "gh-pages"
    Shell.copyRecursive "output/fshafas" "gh-pages" true |> printfn "%A"
    Git.Staging.stageAll "gh-pages"
    Git.Commit.exec "gh-pages" (sprintf "Update generated documentation %s" release.AssemblyVersion)
    Git.Branches.pushBranch "gh-pages" url "gh-pages"
)

Target.create "ReleaseDocsForce" (fun _ ->
    Shell.cleanDir "gh-pages"
    let url = "https://github.com/bergmannjg/fshafas.git"
    Git.Repository.init "gh-pages" false false

    Git.Branches.checkout "gh-pages" true "gh-pages"
    Shell.copyRecursive "output/fshafas" "gh-pages" true |> printfn "%A"
    Git.Staging.stageAll "gh-pages"
    Git.Commit.exec "gh-pages" (sprintf "Update generated documentation %s" release.AssemblyVersion)
    CreateProcess.fromRawCommand "git" ["push"; "--force"; url; "gh-pages"]
    |> CreateProcess.withWorkingDirectory "gh-pages"
    |> Proc.run // start with the above configuration
    |> checkResultUnit "git push failed" 
)

Target.create "BuildLib" (fun _ ->
  DotNet.exec id "build" "src/fshafas/fshafas.fsproj" 
  |> checkResult "buildLib failed"
)

Target.create "BuildWebApp" (fun _ ->
  DotNet.exec id "build" "src/examples/fshafas.fable.web/fsHafasWeb.fsproj" 
  |> checkResult "buildLib failed"
)

Target.create "Test" (fun _ ->
  DotNet.exec id "test" "src/fshafas.test/fshafastest.fsproj"
  |> checkResult "Test failed"
)

Target.create "BuildCSharp" (fun _ ->
  DotNet.exec id "build" "src/examples/fshafas.csharp/fshafas.csharp.csproj" 
  |> checkResult "BuldCSharp failed"
)

Target.create "BuildFableApp" (fun _ ->
  DotNet.exec id "fable" "./src/examples/fshafas.fable.node/fshafas.fable.fsproj --typedArrays false --outDir ./src/fshafas.fable/build"
  |> checkResult "BuldFableApp failed"
)

Target.create "CheckReleaseVersion" (fun _ ->
  let configStr = File.readAsString("src/fshafas.fable.package/fs-hafas-client/package.json") 
  let config = JsonSerializer.Deserialize(configStr)
  if release.AssemblyVersion <> config.version 
  then raise (System.Exception(sprintf "config version, exptected: %s, actual: %s" release.AssemblyVersion config.version))
)

Target.create "BuildFableWebpackNode" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode production --no-devtool --config ./webpack.node.config.js"
  |> checkResult "BuldFableWebpack failed"
)

Target.create "BuildFableWebpackNodeDev" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode development --devtool source-map --config ./webpack.node.config.js" |> ignore
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with NpmFilePath = "/usr/bin/npm"; WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "BuildFableWebpackWebDev" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode development --config ./webpack.web.config.js" |> ignore
)

Target.create "BuildFableNpmPack" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with NpmFilePath = "/usr/bin/npm"; WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "BuildFableNpmPackDev" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with NpmFilePath = "/usr/bin/npm"; WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "CopyWebBundle" (fun _ ->
    Shell.copy "src/examples/fshafas.fable.web/wwwroot/js/lib/" ["src/fshafas.fable.package/fs-hafas-client-web/fshafas.web.bundle.js"]
    Shell.copy "src/examples/fshafas.fable.web/wwwroot/js/lib/" ["src/fshafas.fable.package/fs-hafas-client/hafas-client.d.ts"]
    Shell.copy "src/examples/fshafas.fable.web/wwwroot/js/lib/" ["src/fshafas.fable.package/fs-hafas-client-web/fshafas.web.bundle.d.ts"]
)

let run workingDirectory file arguments = 
      use __ = Trace.traceTask file ""
      let startInfo = new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)
      startInfo.WorkingDirectory <- workingDirectory
      let callResult = 
            startInfo
            |> CreateProcess.ofStartInfo
            |> CreateProcess.redirectOutput
            |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
            |> Proc.run

      if callResult.ExitCode = 0 
      then 
            Trace.trace callResult.Result.Output
      else
            Trace.traceError callResult.Result.Output
            failwith (file + " encountered errors!")

let runWithFailureResult workingDirectory file arguments = 
      use __ = Trace.traceTask file ""
      let startInfo = new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)
      startInfo.WorkingDirectory <- workingDirectory
      let callResult = 
            startInfo
            |> CreateProcess.ofStartInfo
            |> CreateProcess.redirectOutput
            |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
            |> Proc.run

      if callResult.ExitCode = 0 
      then 
            Trace.trace callResult.Result.Output
      else
            Trace.traceError callResult.Result.Output
      
      callResult.ExitCode = 0 

let runWithResult workingDirectory file arguments = 
      use __ = Trace.traceTask file ""
      let startInfo = new Diagnostics.ProcessStartInfo(FileName = file, Arguments = arguments)
      startInfo.WorkingDirectory <- workingDirectory
      let callResult = 
            startInfo
            |> CreateProcess.ofStartInfo
            |> CreateProcess.redirectOutput
            |> CreateProcess.withTimeout (TimeSpan.FromMinutes 5.)
            |> Proc.run

      if callResult.ExitCode = 0 
      then 
            callResult.Result.Output
      else
            Trace.traceError callResult.Result.Output
            callResult.Result.Error

Target.create "CompileTypeScript" (fun _ ->
  run "./src/examples/fshafas.fable.web/wwwroot/js/" "tsc"  ("--target ES2015 --noImplicitAny" + " site.ts")
)

Target.create "PublishToLocalNuGetFeed" (fun _ ->
  DotNet.exec id "pack" "src/fshafas/fshafas.fable.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"
  let release = release.AssemblyVersion
  Shell.cleanDir (home + "/.nuget/packages/fshafas/" + release)
  Shell.cleanDir (home + "/local.packages/fshafas/" + release)
  run "./src/fshafas/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas." + release + ".nupkg" + " -source " + home + "/local.packages -expand")

  DotNet.exec id "pack" "src/fshafas.profiles/fshafas.profiles.fable.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"
  Shell.cleanDir (home + "/.nuget/packages/fshafas.profiles/" + release)
  Shell.cleanDir (home + "/local.packages/fshafas.profiles/" + release)
  run "./src/fshafas.profiles/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas.Profiles." + release + ".nupkg" + " -source " + home + "/local.packages -expand")
)

Target.create "PublishPythonProjToLocalNuGetFeed" (fun _ ->
  DotNet.exec id "pack" "src/fshafas/fshafas.fable.python.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"

  let doc = loadDoc "src/fshafas/fshafas.fable.python.fsproj"
  let release = selectXPathValue "//Version" [("Sdk","Microsoft.NET.Sdk")] doc

  let docProfiles = loadDoc "src/fshafas.profiles/fshafas.profiles.fable.python.fsproj"
  let releaseProfiles = selectXPathValue "//Version" [("Sdk","Microsoft.NET.Sdk")] docProfiles

  if release <> releaseProfiles then failwith "fsproj vesion mismatch"

  Shell.cleanDir (home + "/.nuget/packages/fshafas.python/" + release)
  Shell.cleanDir (home + "/local.packages/fshafas.python/" + release)
  run "./src/fshafas/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas.Python." + release + ".nupkg" + " -source " + home + "/local.packages -expand")

  DotNet.exec id "pack" "src/fshafas.profiles/fshafas.profiles.fable.python.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"
  Shell.cleanDir (home + "/.nuget/packages/fshafas.profiles.python/" + release)
  Shell.cleanDir (home + "/local.packages/fshafas.profiles.python/" + release)
  run "./src/fshafas.profiles/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas.Profiles.Python." + release + ".nupkg" + " -source " + home + "/local.packages -expand")
)

Target.create "InstallPythonPackageFromLocal" (fun _ ->
  let projDir = "src/fshafas.python.package/"
  Shell.cleanDir (projDir + "fable_modules/")
  DotNet.exec id "fable-py" (projDir + "empty.fsproj") |> ignore
  Shell.cleanDir (projDir + "fshafas/fable_modules/")
  Shell.deleteDir (projDir + "fshafas/fable_modules/")
  Shell.mv (projDir + "fable_modules/") (projDir + "fshafas")
  Shell.Exec("bash", "fixes.sh", projDir) |> ignore
  Shell.Exec("python3", "-m pip install -e " + projDir) |> ignore
  Shell.Exec("python3", "-m build " + projDir) |> ignore
)

let replaceRuntimeMgs (s:string) = 
  let replace (pattern:string) (s:string) = 
      Regex.Replace(s, pattern, "")

  s
  |> replace "reg, replacement.*\n"
  |> replace "HH\n"
  |> replace "mm\n"
  |> replace "ss\n"
  |> replace "MM\n"
  |> replace "dd\n"
  |> replace "yyyy\n"
  |> replace "currentLocation.*\n"

let compareNetJsResult (method:string) (args:string) =
  let resultJs = 
    runWithResult  "./src/examples/cli/" "node" ("build/Program.js" + " --" + method + " " + args)
    |> replaceRuntimeMgs

  Trace.trace resultJs

  let jsFile = "./tmp/" + method + "-js.txt"
  System.IO.File.WriteAllText(jsFile, resultJs)

  let resultNet = 
    runWithResult  "./src/examples/cli/" "dotnet" ("run --project cli.fsproj -- " + " --" + method + " " + args)
    |> replaceRuntimeMgs

  Trace.trace resultNet

  let netFile = "./tmp/" + method + "-net.txt"
  System.IO.File.WriteAllText(netFile, resultNet)

  let ok = runWithFailureResult "." "diff" (netFile + " " + jsFile)

  (method, ok)

let compareJsPyResult (method:string) (args:string) =
  let resultJs = 
    runWithResult  "./src/examples/cli/" "node" ("build/Program.js" + " --" + method + " " + args)
    |> replaceRuntimeMgs

  Trace.trace resultJs

  let jsFile = "./tmp/" + method + "-js.txt"
  System.IO.File.WriteAllText(jsFile, resultJs)

  let resultPy = 
    runWithResult  "./src/examples/cli/" "python3" ("program.py" + " --" + method + " " + args)
    |> replaceRuntimeMgs

  Trace.trace resultPy

  let pyFile = "./tmp/" + method + "-py.txt"
  System.IO.File.WriteAllText(pyFile, resultPy)

  let ok = runWithFailureResult "." "diff" (pyFile + " " + jsFile)

  (method, ok)

let tests = [|
      ("locations", "Hannover")
      ("stop", "8000152")
      ("journeys", "8000152 8000036")
      ("journeysfromtrip", "8002549 8000261 8000207")
      ("departures", "8000036")
      ("trips", "\"ICE 1001\"")
      ("nearby", "13.078028 54.308438")
      ("reachablefrom", "13.078028 54.308438")
      ("radar", "52.039421 8.522777 52.019421 8.542777")
    |]

Target.create "CompareJsPyResult" (fun _ ->
  Shell.mkdir "./tmp"
  Shell.cleanDir "./tmp"

  let results = 
    tests
    |> Array.map (fun (m,a) -> compareJsPyResult m a)
    |> Array.map (fun (m,r) -> sprintf "|%s|%s|" m (if r then "ok" else "failed") )
    |> String.concat "\n"

  System.IO.File.WriteAllText("./tmp/results.txt", results)
)

Target.create "CompareNetJsResult" (fun _ ->
  Shell.mkdir "./tmp"
  Shell.cleanDir "./tmp"

  let results = 
    tests
    |> Array.map (fun (m,a) -> compareNetJsResult m a)
    |> Array.map (fun (m,r) -> sprintf "|%s|%s|" m (if r then "ok" else "failed") )
    |> String.concat "\n"

  System.IO.File.WriteAllText("./tmp/results.txt", results)
)

open Fake.Core.TargetOperators

Target.create "Default" ignore

Target.create "Debug" ignore

Target.create "Python" ignore

Target.create "Web" ignore

Target.create "Docs" ignore

"BuildLib"
==> "Test"
==> "CheckReleaseVersion"
==> "PublishToLocalNuGetFeed"
==> "BuildFableWebpackNode"
==> "BuildFableNpmPack"
==> "Default"

"BuildLib"
==> "Test"
==> "CheckReleaseVersion"
==> "PublishToLocalNuGetFeed"
==> "BuildFableWebpackNodeDev"
==> "BuildFableNpmPackDev"
==> "Debug"

"BuildLib"
==> "PublishPythonProjToLocalNuGetFeed"
==> "InstallPythonPackageFromLocal"
==> "Python"

"BuildLib"
==> "BuildDocs"
==> "Docs"

"BuildFableWebpackWebDev"
==> "CopyWebBundle"
==> "CompileTypeScript"
==> "BuildWebApp"
==> "Web"

Target.runOrDefault "Default"
