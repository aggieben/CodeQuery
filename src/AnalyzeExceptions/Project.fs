module Project

open System
open Microsoft.CodeAnalysis

type MSBuildProjectProperty = Microsoft.Build.Evaluation.ProjectProperty
type MSBuildProject = Microsoft.Build.Evaluation.Project

let getReferencedProjectIds (proj:Project) =
    match List.ofSeq proj.ProjectReferences with
    | [] -> []
    | prl -> List.map (fun (pr:ProjectReference) -> pr.ProjectId.Id) prl

let appProjectTypes = set [Guid.Parse("{349c5851-65df-11da-9384-00065b846f21}"); // web application
                           Guid.Parse("{E3E379DF-F4C6-4180-9B81-6769533ABE47}"); // mvc 4
                           ]
let hasProjectType (guids:Set<Guid>) (msbproj:MSBuildProject) =
    match Seq.tryFind (fun (prop:MSBuildProjectProperty) -> prop.Name = "ProjectTypeGuids") msbproj.Properties with
    | None -> false
    | Some prop -> prop.EvaluatedValue.Split([|';'|], StringSplitOptions.RemoveEmptyEntries)
                   |> Array.map (fun s -> Guid.Parse(s)) |> set
                   |> Set.intersect guids
                   |> Set.isEmpty |> not

let hasExeOutputType (msbproj:MSBuildProject) =
    match Seq.tryFind (fun (prop:MSBuildProjectProperty) -> prop.Name = "OutputType") msbproj.Properties with
    | None -> false
    | Some prop -> 
        let value = prop.EvaluatedValue.ToLower()
        value = "exe" || value = "winexe"

let getAppProjects (projects:Project seq) =
    let projectMap = projects |> Seq.fold (fun map p -> Map.add p.Id.Id (MSBuildProject(p.FilePath)) map) Map.empty
    
    let projectIds = projects
                     |> Seq.filter (fun p -> (p.Name.ToLower()).Contains("test") |> not)
                     |> Seq.map (fun p -> projectMap.[p.Id.Id])
                     |> Seq.filter (fun mp -> (hasExeOutputType mp) || (hasProjectType appProjectTypes mp))
                     |> Seq.map (fun mp -> Map.pick (fun key msproj -> if msproj = mp then Some key else None) projectMap)

    projects |> Seq.filter (fun p -> Seq.contains p.Id.Id projectIds)

let getRootProjects (projects:Project seq) =
    let referencedProjects = projects 
                             |> Seq.map getReferencedProjectIds 
                             |> Seq.fold (fun state pidl -> Set.union (Set.ofSeq pidl) state) Set.empty
    projects |> Seq.filter (fun p -> Set.contains p.Id.Id referencedProjects |> not)