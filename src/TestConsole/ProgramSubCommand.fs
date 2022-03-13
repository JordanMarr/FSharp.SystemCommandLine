module ProgramSubCommand

open FSharp.SystemCommandLine

let listCmd = 
    let handler (path: string) = 
        printfn $"The path is {path}."
        
    let oPath = Opt("--path", getDefaultValue = (fun () -> "/"), description = "The path to list")

    command "list" {
        description "lists contents of a folder"
        options (oPath)
        setHandler handler
    }

//[<EntryPoint>]
let main argv = 
    rootCommand {
        description "My sample app"
        setHandler id
        setCommand listCmd
    }
    
        
