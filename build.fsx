#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MsBuild
nuget Fake.JavaScript.Npm
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
open Fake.Tools
open Fake.Core.TargetOperators
open Fake.IO.Globbing.Operators
open Fake.JavaScript
open Fake.DotNet

let projectFiles = !! "src/*/*.*proj"

Target.create "Clean" (fun _ ->
    Shell.cleanDir "src/fshafas.fable/build"
    Shell.cleanDir "src/fshafas.fable.package/build"
    Shell.rm "src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.js"
    Shell.rm "src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.js.map"
    projectFiles |> Seq.iter (fun x -> DotNet.exec id "clean" x |> ignore)
)

let checkResult (msg:string) (res: ProcessResult) =
  if not res.OK then failwith msg

Target.create "BuildDocs" (fun _ ->
  DotNet.exec id "fsdocs" "build --clean --projects src/fshafas/fshafas.fsproj --input src/fshafas/docs/ --output output/fshafas" |> ignore
)

Target.create "ReleaseDocs" (fun _ ->
    Shell.cleanDir "gh-pages"
    let url = "https://github.com/bergmannjg/fshafas.git"
    Git.Repository.cloneSingleBranch "" url "gh-pages" "gh-pages"

    // Git.Repository.fullclean "gh-pages"
    Shell.copyRecursive "output/fshafas" "gh-pages" true |> printfn "%A"
    Git.Staging.stageAll "gh-pages"
    Git.Commit.exec "gh-pages" (sprintf "Update generated documentation")
    Git.Branches.pushBranch "gh-pages" url "gh-pages"
)

Target.create "BuildLib" (fun _ ->
  DotNet.exec id "build" "src/fshafas/fshafas.fsproj" 
  |> checkResult "buildLib failed"
)

Target.create "Test" (fun _ ->
  DotNet.exec id "test" "src/fshafas.test/fshafastest.fsproj"
  |> checkResult "Test failed"
)

Target.create "BuildCSharp" (fun _ ->
  DotNet.exec id "build" "src/fshafas.csharp/fshafas.csharp.csproj" 
  |> checkResult "BuldCSharp failed"
)

Target.create "BuildFableApp" (fun _ ->
  DotNet.exec id "fable" "./src/fshafas.fable/fshafas.fable.fsproj --typedArrays false --outDir ./src/fshafas.fable/build"
  |> checkResult "BuldFableApp failed"
)

Target.create "BuildFableWebpack" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode production --config ./webpack.config.js"
  |> checkResult "BuldFableWebpack failed"
)

Target.create "BuildFableWebpackDev" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode development --devtool source-map --config ./webpack.config.js" |> ignore
)

Target.create "BuildFableNpmPack" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

open Fake.Core.TargetOperators

Target.create "Default" ignore

Target.create "Docs" ignore

"BuildLib"
==> "Test"
==> "BuildCSharp"
==> "BuildFableApp"
==> "BuildFableWebpack"
==> "BuildFableNpmPack"
==> "Default"

"BuildLib"
==> "BuildDocs"
==> "Docs"

Target.runOrDefault "Default"
