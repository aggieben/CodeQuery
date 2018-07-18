#load ".fake/build.fsx/intellisense.fsx"
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs 
)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj"
    -- "src/CodeQuery/CodeQuery.fsproj" // this project is broken at the moment.
    |> Seq.iter (DotNet.build id)
)

Target.create "AnalyzeExceptions" (fun _ ->
    "src/AnalyzeExceptions/AnalyzeExceptions.fsproj"
    |> DotNet.publish (fun c -> 
        { c with
            OutputPath = Some (__SOURCE_DIRECTORY__ @@ "build" @@ "AnalyzeExceptions")
        })
)

Target.create "All" ignore

"Clean"
  ==> "Build"
  ==> "All"

"Clean"
  ==> "Build"
  ==> "AnalyzeExceptions"

Target.runOrDefault "AnalyzeExceptions"
