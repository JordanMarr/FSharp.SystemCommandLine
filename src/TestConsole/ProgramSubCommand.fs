module ProgramSubCommand

open System.IO
open FSharp.SystemCommandLine

let listCmd = 
    let handler (dir: DirectoryInfo) = 
        if dir.Exists 
        then dir.EnumerateFiles() |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else printfn $"{dir.FullName} does not exist."
        
    let dir = Input.Argument(getDefaultValue = (fun () -> DirectoryInfo("c:\fake dir")))

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setHandler handler
    }

let deleteCmd = 
    let handler (dir: DirectoryInfo, recursive: bool) = 
        if dir.Exists then 
            if recursive
            then printfn $"Recursively deleting {dir.FullName}"
            else printfn $"Deleting {dir.FullName}"
        else 
            printfn $"{dir.FullName} does not exist."

    let dir = Input.Argument(getDefaultValue = (fun () -> DirectoryInfo("c:\fake dir")))    
    let recursive = Input.Option("--recursive", getDefaultValue = (fun () -> false))

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setHandler handler
    }
        

//[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "File System Manager"
        setHandler id
        setCommand listCmd
        setCommand deleteCmd
    }
    
        
