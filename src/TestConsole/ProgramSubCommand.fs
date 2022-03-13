module ProgramSubCommand

open System.IO
open System.CommandLine.Binding
open FSharp.SystemCommandLine

let listCmd = 
    let handler (path: string) = 
        printfn $"The path is {path}."
        
    let oPath = Opt("--path", getDefaultValue = (fun () -> "/"), description = "The path to list") :> IValueDescriptor<_>

    command "list" {
        description "lists contents of a folder"
        inputs (oPath)
        setHandler handler
    }

//[<EntryPoint>]
let main argv = 
    rootCommand {
        description "My sample app"
        setHandler id
        setCommand listCmd
    }
    
        
