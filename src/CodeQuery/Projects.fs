module Project
open System
open Microsoft.CodeAnalysis

type ProjectMap = Map<Guid,Project>

let getRootProjects (projects:Project seq) =
    printfn "searching %d projects for roots" (Seq.length projects)
    let referencedProjects = projects |> Seq.fold (fun set p -> Set.add p.Id.Id set) Set.empty
    projects |> Seq.filter (fun p -> not (Seq.contains p.Id.Id referencedProjects))

let getProjectMap (projects:Project seq) =
    projects |> Seq.fold (fun map proj -> Map.add proj.Id.Id proj map) Map.empty

let projectName (projectMap:ProjectMap) projectId =
    projectMap.[projectId] |> (fun p -> p.Name)
