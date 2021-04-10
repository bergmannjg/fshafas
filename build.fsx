#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MsBuild
nuget Fake.JavaScript.Npm
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.IO
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

Target.create "BuildLib" (fun _ ->
  DotNet.exec id "build" "src/fshafas/fshafas.fsproj" |> ignore
)

Target.create "Test" (fun _ ->
  DotNet.exec id "test" "src/fshafas.test/fshafastest.fsproj" |> ignore
)

Target.create "BuldCSharp" (fun _ ->
  DotNet.exec id "build" "src/fshafas.csharp/fshafas.csharp.csproj" |> ignore
)

Target.create "BuldFableApp" (fun _ ->
  DotNet.exec id "fable" "./src/fshafas.fable/fshafas.fable.fsproj --typedArrays false --outDir ./src/fshafas.fable/build" |> ignore
)

Target.create "BuldFableWebpack" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode production --config ./webpack.config.js" |> ignore
)

Target.create "BuldFableWebpackDev" (fun _ ->
  DotNet.exec (DotNet.Options.withWorkingDirectory "./src/fshafas.fable.package") "fable" "./fshafas.fable.fsproj --typedArrays false --define WEBPACK --outDir ./build --run webpack --mode development --devtool source-map --config ./webpack.config.js" |> ignore
)

Target.create "BuldFableNpmPack" (fun _ ->
  Npm.exec "pack fs-hafas-client/" (fun o -> { o with WorkingDirectory = "./src/fshafas.fable.package/" }) |> ignore
)

Target.create "Default" ignore

"BuildLib"
==> "Test"
==> "BuldCSharp"
==> "BuldFableApp"
==> "BuldFableWebpack"
==> "BuldFableNpmPack"
==> "Default"

Target.runOrDefault "Default"