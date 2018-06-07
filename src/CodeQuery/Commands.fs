module Commands
open System.Diagnostics
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Workspaces
open System

type CmdOutput = {
    text: string;
    time: int64 option;
}

type InteractiveCommand = MSBuildWorkspace -> Async<CmdOutput>

let topHelp = """
commands:
    help    print this help
    sln     solution-related commands
"""

let slnHelp = """
sln <command> [args]
commands:
    help           prints this help
    open <path>    opens solution at path"
"""

let slnOpenHelp =
    "sln open <path>"

let projHelp = """
proj <command> [args]
commands:
    help                 prints this help
    list [[!]pattern]    list open projects, optionally with filter, which can be negated with !
"""

let projListHelp = """
proj list [[!]pattern] - pattern is just a substring; ! will match project names that don't match the pattern.
"""

let openSolution (workspace:MSBuildWorkspace) path = async {
    // TODO: use IProgress<ProjectLoadProgress> as soon as Roslyn 2.9 is out

    let sw = Stopwatch.StartNew()
    let! sln = workspace.OpenSolutionAsync(path) |> Async.AwaitTask
    sw.Stop()

    return {
        text = sprintf "Opened %s (%d projects)" path (Seq.length sln.Projects);
        time = Some sw.ElapsedMilliseconds
    }
}

let listProjects (workspace:MSBuildWorkspace) pattern = async {
    let (|Prefix|_|) (p:string) (s:string) =
        if s.StartsWith(p) then Some(s.Substring(p.Length))
        else None

    let sw = Stopwatch.StartNew()
    let projectFilter (name:string) = match pattern with
                                      | Prefix "!" substring  -> name.Contains(substring) |> not
                                      | substring -> name.Contains(substring)

    let result = workspace.CurrentSolution.Projects
                 |> Seq.filter (fun p -> projectFilter p.Name)
                 |> Seq.map (fun p -> p.Name)
                 |> String.concat "\n"
    sw.Stop()
    
    return {
        text = result; time = Some sw.ElapsedMilliseconds
    }
}

let parseSlnCommand (inputs:string list) =
    match inputs with
    | "help"::_ -> 
        fun ws -> async {
            ws |> ignore
            return { text = slnHelp; time = None }
        }
    | "open"::rest ->
        match rest with
        | [] -> 
            fun ws -> async {
                ws |> ignore
                return { text = sprintf "invalid arguments.\n%s" slnOpenHelp; time = None }
            }
        | path::_ -> 
            fun ws -> openSolution ws path
    | _ -> 
        fun ws -> async {
            ws |> ignore
            return { text = sprintf "unrecognized command.\n%s" slnHelp; time = None }
        }

let parseProjCommand (inputs:string list) =
    match inputs with
    | "help"::_ ->
        fun ws -> async {
            ws |> ignore
            return { text = projHelp; time = None }
        }
    | "list"::rest ->
        match rest with
        | [] ->
            fun ws -> listProjects ws String.Empty
        | pattern::_ ->
            fun ws -> listProjects ws pattern
    | _ ->
        fun ws -> async {
            ws |> ignore
            return { text = sprintf "unrecognized command.\n%s" projHelp; time = None }
        }

let parseCommand (input:string) =
    let tokens = input.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) |> Array.toList
    match tokens with
    | [] ->
        fun ws -> async {
            ws |> ignore
            return { text = String.Empty; time = None }
        }

    | "help"::_ -> 
        fun ws -> async {
            ws |> ignore
            return { text = topHelp; time = None }
        }

    | "sln"::rest -> parseSlnCommand rest

    | "proj"::rest -> parseProjCommand rest

    | _ ->
        fun ws -> async {
            ws |> ignore
            return { text = sprintf "unrecognized command.\n%s" topHelp; time = None }
        }