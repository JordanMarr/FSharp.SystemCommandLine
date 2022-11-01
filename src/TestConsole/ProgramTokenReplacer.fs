module ProgramTokenReplacer

open FSharp.SystemCommandLine
open System.CommandLine.Parsing

let app (package: string) =
    if package.StartsWith("@") then
        printfn $"{package}"
        0
    else
        eprintfn "The package name does not start with a leading @"
        1

//[<EntryPoint>]
let main argv =
    let package =
        Input.Option([ "--package"; "-p" ], "A package with a leading @ name")

    rootCommand argv {
        description "Can be called with a leading @ package"

        useTokenReplacer (
            // in this case we want to skip @ processing
            TryReplaceToken (fun _ _ _ -> false)
        )

        inputs package
        setHandler app
    }
