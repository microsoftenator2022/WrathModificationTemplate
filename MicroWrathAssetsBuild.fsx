open System.Diagnostics
open System.IO

let unityPath = @"C:\Program Files\Unity\Hub\Editor\2020.3.48f1\Editor\Unity.exe"

if unityPath |> (not << File.Exists) then failwith $"Unity not found at {unityPath}"

let args = fsi.CommandLineArgs
let scriptDirectory = args[0] |> Path.GetDirectoryName

let setup = $"""-quit -batchmode -nographics -ignoreCompilerErrors -executeMethod "OwlcatModification.Editor.Setup.ProjectSetup.MicroWrathProjectSetup" -projectPath"""
let build = $"""-quit -batchmode -nographics -executeMethod "OwlcatModification.Editor.Build.Builder.BuildMicroWrathAssets" -projectPath"""

let run args =
    use proc = new Process()
    let psi = ProcessStartInfo()

    psi.FileName <- unityPath
    psi.Arguments <- $"{args} {scriptDirectory}"

    proc.StartInfo <- psi

    proc.Start() |> ignore

    proc.WaitForExit()

    proc.ExitCode

printfn ""
printfn "Setup Project..."
let src = run setup
if src <> 0 then failwith $"Error code {src} in setup"
printfn ""
printfn "Setup Complete"

printfn ""
printfn "Build Project..."
let brc = run build
if brc <> 0 then failwith $"Error code {brc} in build"
printfn ""
printfn "Build Complete"
