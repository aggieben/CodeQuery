module ExceptionHandler

open Microsoft.CodeAnalysis
open System.Threading
open Microsoft.CodeAnalysis.CSharp.Syntax

let hasExceptionDeclaration exceptionType (ccSyntax:CatchClauseSyntax) =
    if isNull ccSyntax.Declaration then false
    else ccSyntax.Declaration.Type.ToString() = exceptionType

let hasExceptionFilter exceptionType (ccSyntax:CatchClauseSyntax) =
    if isNull ccSyntax.Filter then false
    else ccSyntax.Filter.DescendantNodesAndSelf()
         |> Seq.exists (fun node -> node :? TypeSyntax && (node :?> TypeSyntax).ToString() = exceptionType)

let usesExceptionInBlock exceptionType (ccSyntax:CatchClauseSyntax) =
    let isTypeSyntaxOfType (t:string) (node:SyntaxNode) = node :? TypeSyntax && (node :?> TypeSyntax).ToString() = t
    let isInNewObjectExpression (node:TypeSyntax) = node.Ancestors() |> Seq.exists (fun sn -> sn :? ObjectCreationExpressionSyntax)

    let allNodes = ccSyntax.Block.DescendantNodesAndSelf()
    let typeNodes = Seq.filter (isTypeSyntaxOfType exceptionType) allNodes
                    |> Seq.map (fun node -> node :?> TypeSyntax)

    Seq.exists (fun node -> isTypeSyntaxOfType exceptionType node
                            && (not << isInNewObjectExpression) node) typeNodes

let isExceptionCatchClause exceptionType (ccSyntax:CatchClauseSyntax) =
    hasExceptionDeclaration exceptionType ccSyntax 
    || hasExceptionFilter exceptionType ccSyntax
    || usesExceptionInBlock exceptionType ccSyntax

let hasCatchClause exceptionType (trySyntax:TryStatementSyntax) =
    trySyntax.ChildNodes() 
    |> Seq.filter (fun child -> child :? CatchClauseSyntax) 
    |> Seq.map (fun node -> node :?> CatchClauseSyntax)
    |> Seq.filter (isExceptionCatchClause exceptionType)
    |> (Seq.isEmpty >> not)

let getContainingSymbol (compilation:Compilation) (syntaxNode:SyntaxNode) =
    let model = compilation.GetSemanticModel(syntaxNode.SyntaxTree)
    model.GetEnclosingSymbol(syntaxNode.SpanStart)

/// Finds try statements that have catch clauses that handle the includeExn exception type, but not the excludeExn exception type if provided.
let analyzeHandlers includeExn excludeExn (project:Project) = async {
    let! compilation = project.GetCompilationAsync() |> Async.AwaitTask

    return compilation.SyntaxTrees 
    |> Seq.collect (fun stree -> stree.GetRoot(CancellationToken.None).DescendantNodesAndSelf() |> Seq.filter (fun node -> node :? TryStatementSyntax))
    |> Seq.map (fun node -> node :?> TryStatementSyntax)
    |> Seq.filter (hasCatchClause includeExn)
    |> match excludeExn with
       | None -> id
       | Some exn -> Seq.filter ((hasCatchClause exn) >> not)
    |> Seq.map (fun cc -> (cc, getContainingSymbol compilation cc))
}
