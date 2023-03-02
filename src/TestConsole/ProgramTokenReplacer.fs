module ProgramTokenReplacer

open FSharp.SystemCommandLine

let app (package: string) =
    if package.StartsWith("@") then
        printfn $"{package}"
        0
    else
        eprintfn "The package name does not start with a leading @"
        1

//[<EntryPoint>]
let main argv =

    // The package option needs to accept strings that start with "@" symbol.
    // For example, "--package @shoelace-style/shoelace".
    // To accomplish this, we will need to modify the default pipeline settings below.
    let package = Input.Option<string>([ "--package"; "-p" ], "A package with a leading @ name")

    rootCommand argv {
        description "Can be called with a leading @ package"

        usePipeline (fun builder -> 
            // Skip @ processing
            builder.UseTokenReplacer(fun _ _ _ -> false)
        )
        
        inputs package
        setHandler app
    }
