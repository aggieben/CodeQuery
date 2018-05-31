module CodeQuery

open System
open System.Collections.Generic
open System.Diagnostics
open Microsoft.Build.Locator
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Workspaces

type ProjectMap = Map<Guid,Project>

let getRootProjects (projects:Project seq) =
    printfn "searching %d projects for roots" (Seq.length projects)
    let referencedProjects = projects |> Seq.fold (fun set p -> Set.add p.Id.Id set) Set.empty
    projects |> Seq.filter (fun p -> not (Seq.contains p.Id.Id referencedProjects))

let getProjectMap (projects:Project seq) =
    projects |> Seq.fold (fun map proj -> Map.add proj.Id.Id proj map) Map.empty

let projectName (projectMap:ProjectMap) projectId =
    projectMap.[projectId] |> (fun p -> p.Name)

[<EntryPoint>]
let main argv =
    MSBuildLocator.RegisterDefaults() |> ignore

    use workspace = MSBuildWorkspace.Create()

    printf "Opening solution %s..." argv.[0]
    let sw = Stopwatch.StartNew()
    let solution = workspace.OpenSolutionAsync(argv.[0]) |> Async.AwaitTask |> Async.RunSynchronously
    sw.Stop()

    printfn "done. [%02f s]" (float sw.ElapsedMilliseconds / 1000.0)

    printfn "Loaded %d projects" (Seq.length solution.Projects)
    let allProjectsMap = getProjectMap solution.Projects

    let rootProjects = solution.Projects 
                        |> Seq.filter (fun p -> not (p.Name.Contains("Test")))
                        |> getRootProjects
    
    for proj in rootProjects do
        printfn "%A" proj.Name

    0 // return an integer exit code
