module CodeQuery

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.Build.Locator
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Workspaces

open Commands

let renderPrompt() =
    printf "> "

let rec commandLoop workspace =
    renderPrompt()

    let userInput = Console.ReadLine()
    let cmd = parseCommand userInput
    let output = cmd workspace |> Async.RunSynchronously

    match output.time with
    | Some t -> printfn "%s\n[%d ms]" output.text t
    | None -> match output.text with
              | "" -> ()
              | _ -> printfn "%s\n" output.text

    commandLoop workspace

[<EntryPoint>]
let main argv =
    argv |> ignore
    MSBuildLocator.RegisterDefaults() |> ignore
    use workspace = MSBuildWorkspace.Create()

    commandLoop workspace

    // printf "Opening solution %s..." argv.[0]
    // let sw = Stopwatch.StartNew()
    // let solution = workspace.OpenSolutionAsync(argv.[0]) |> Async.AwaitTask |> Async.RunSynchronously
    // sw.Stop()

    // printfn "done. [%02f s]" (float sw.ElapsedMilliseconds / 1000.0)

    // printfn "Loaded %d projects" (Seq.length solution.Projects)
    // let allProjectsMap = getProjectMap solution.Projects

    // let rootProjects = solution.Projects 
    //                     |> Seq.filter (fun p -> not (p.Name.Contains("Test")))
    //                     |> getRootProjects
    
    // for proj in rootProjects do
    //     printfn "%A" proj.Name

    0 // return an integer exit code
