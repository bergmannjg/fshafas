#r "paket:
nuget FSharp.SystemTextJson
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MsBuild
nuget Fake.JavaScript.Npm
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
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
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "BuildFableWebpackWebDev" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode development --config ./webpack.web.config.js" |> ignore
)

Target.create "BuildFableNpmPack" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "BuildFableNpmPackDev" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
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

Target.create "CompileTypeScript" (fun _ ->
  run "./src/examples/fshafas.fable.web/wwwroot/js/" "tsc"  ("--target ES2015 --noImplicitAny" + " site.ts")
)

Target.create "PublishToLocalFeed" (fun _ ->
  DotNet.exec id "pack" "src/fshafas/fshafas.fable.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"
  Shell.cleanDir (home + "/.nuget/packages/fshafas")
  Shell.cleanDir (home + "/local.packages/fshafas")
  run "./src/fshafas/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas." + release.AssemblyVersion + ".nupkg" + " -source " + home + "/local.packages -expand")

  DotNet.exec id "pack" "src/fshafas.profiles/fshafas.profiles.fable.fsproj" 
  |> checkResult "pack failed"
  let home = Environment.environVar "HOME"
  Shell.cleanDir (home + "/.nuget/packages/fshafas.profiles")
  Shell.cleanDir (home + "/local.packages/fshafas.profiles")
  run "./src/fshafas.profiles/" "/usr/local/bin/nuget.exe" ("add" + " bin/Debug/FsHafas.Profiles." + release.AssemblyVersion + ".nupkg" + " -source " + home + "/local.packages -expand")
)

open Fake.Core.TargetOperators

Target.create "Default" ignore

Target.create "Debug" ignore

Target.create "Web" ignore

Target.create "Docs" ignore

"BuildLib"
==> "Test"
==> "CheckReleaseVersion"
==> "PublishToLocalFeed"
==> "BuildFableWebpackNode"
==> "BuildFableNpmPack"
==> "Default"

"BuildLib"
==> "Test"
==> "CheckReleaseVersion"
==> "PublishToLocalFeed"
==> "BuildFableWebpackNodeDev"
==> "BuildFableNpmPackDev"
==> "Debug"

"BuildLib"
==> "BuildDocs"
==> "Docs"

"BuildFableWebpackWebDev"
==> "CopyWebBundle"
==> "CompileTypeScript"
==> "BuildWebApp"
==> "Web"

Target.runOrDefault "Default"
