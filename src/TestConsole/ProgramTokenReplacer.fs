module ProgramTokenReplacer

module SCL = 
    open System.CommandLine

    let packageOpt = Option<string>("--package", [|"-p"|], Description = "A package with a leading @ name")

    let action (parseResult: ParseResult) = 
        let package = parseResult.GetValue packageOpt
        printfn $"The value for --package is: %s{package}"

    let main (argv: string[]) = 
        let cfg = new CommandLineConfiguration(new RootCommand())
        cfg.ResponseFileTokenReplacer <- null // in beta5, you must set ResponseFileTokenReplacer to null to skip @ processing
        let cmd = cfg.RootCommand

        cmd.Description <- "My sample app"
        cmd.Add(packageOpt)
        cmd.SetAction(action)

        cmd.Parse(argv).Invoke()


open FSharp.SystemCommandLine
open Input
open System.CommandLine.Parsing

let main argv =
    let app (package: string) =
        if package.StartsWith("@") then
            printfn $"{package}"
            0
        else
            eprintfn "The package name does not start with a leading @"
            1

    rootCommand argv {
        description "Can be called with a leading @ package"
        configure (fun cfg -> 
            // Skip @ processing
            //cfg.UseTokenReplacer(fun _ _ _ -> false)
            //cfg.ResponseFileTokenReplacer <- null // in beta5, you must set ResponseFileTokenReplacer to null to skip @ processing
            cfg.ResponseFileTokenReplacer <- new TryReplaceToken(fun _ _ _ -> true)
            cfg
        )        
        inputs (
            // The package option needs to accept strings that start with "@" symbol.
            // For example, "--package @shoelace-style/shoelace".
            // To accomplish this, we will need to modify the default pipeline settings below.
            option<string> "--package" 
            |> aliases ["-p"] 
            |> desc "A package with a leading @ name"
            |> editOption (fun o ->
                o.CustomParser <- (fun res -> 
                    // Custom parser to allow leading @ in the package name
                    let value = res.GetValueOrDefault<string>()
                    if value.StartsWith("@") 
                    then value
                    else 
                        res.AddError("oops")
                        "---"
                )
            )
        )
        setAction app
    }
