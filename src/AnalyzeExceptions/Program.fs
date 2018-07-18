module Program
open Argu
open Microsoft.Build.Locator
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open ExceptionHandler
open System

type Arguments =
    | Solution of path:string
    | [<Mandatory>] IncludeException of includeExn:string
    | ExcludeException of exludeExn:string option
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with 
            | Solution _ -> "specify the path of the solution to analyze"
            | IncludeException _ -> "specify the exception type name to include in the analysis"
            | ExcludeException _ -> "specify optional exception type name to exclude in the analysis"

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<Arguments>(programName = "AnalyzeExceptions.exe", errorHandler = errorHandler)
    let parsedArgs = parser.ParseCommandLine argv

    MSBuildLocator.RegisterDefaults() |> ignore
    use workspace = MSBuildWorkspace.Create()

    let analysisSolution = parsedArgs.GetResult(Solution)
    let solution = workspace.OpenSolutionAsync(analysisSolution) 
                    |> Async.AwaitTask 
                    |> Async.RunSynchronously

    printfn "Opened %d projects." (Seq.length solution.Projects)

    let nonTestProjects = solution.Projects
                          |> Seq.filter (fun p -> p.Name.Contains("Test") |> not)
    printfn "Found %d non-test projects." (Seq.length nonTestProjects)

    let appProjects = nonTestProjects |> Project.getAppProjects
    printfn "Found %d app projects" (Seq.length appProjects)

    let includeExn = parsedArgs.GetResult(IncludeException)
    let excludeExn = parsedArgs.GetResult(ExcludeException, None)

    let ccLocations = nonTestProjects 
                      |> Seq.map (analyzeHandlers includeExn excludeExn)
                      |> Async.Parallel
                      |> Async.RunSynchronously
                      |> Seq.concat

    printfn "found %d relevant catch expressions" (Seq.length ccLocations)

    for (cc,symbol) in ccLocations do
        let flps = cc.GetLocation().GetLineSpan()
        let symbolString = symbol.ToString()
        printfn "%s | %s:%d" symbolString flps.Path flps.StartLinePosition.Line

    0 // return an integer exit code