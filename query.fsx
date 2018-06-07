#r "./packages/Microsoft.Build.Locator/lib/net46/Microsoft.Build.Locator.dll"
// #r "./packages/Microsoft.CodeAnalysis.Common/lib/netstandard1.3/Microsoft.CodeAnalysis.dll"
// #r "./packages/Microsoft.CodeAnalysis.CSharp.Workspaces/lib/netstandard1.3/Microsoft.CodeAnalysis.CSharp.Workspaces.dll"
// #r "./packages/Microsoft.CodeAnalysis.Workspaces.Common/lib/net46/Microsoft.CodeAnalysis.Workspaces.dll"
// #r "./packages/Microsoft.CodeAnalysis.Workspaces.Common/lib/net46/Microsoft.CodeAnalysis.Workspaces.Desktop.dll"
// #r "./packages/System.Composition.TypedParts/lib/netstandard2.0/System.Composition.TypedParts.dll"
// #r "./packages/System.Composition.Hosting/lib/netstandard2.0/System.Composition.Hosting.dll"
// #r "./packages/System.Composition.Runtime/lib/netstandard2.0/System.Composition.Runtime.dll"
// #r "./packages/System.Composition.AttributedModel/lib/netstandard2.0/System.Composition.AttributedModel.dll"

// open Microsoft.Build.Locator
// open Microsoft.CodeAnalysis.MSBuild

Microsoft.Build.Locator.MSBuildLocator.RegisterDefaults() |> ignore
// let workspace = MSBuildWorkspace.Create()