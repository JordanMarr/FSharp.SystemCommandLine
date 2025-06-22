module ProgramSubCommand

open System.IO
open FSharp.SystemCommandLine
open Input

let listCmd =
    let action (dir: DirectoryInfo) =
        if dir.Exists then
            dir.EnumerateFiles()
            |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else
            printfn $"{dir.FullName} does not exist."

    let dir = argument "directory" |> defVal (DirectoryInfo @"c:\default")

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setAction action
        addAlias "ls"
    }

let deleteCmd =
    let action (dir: DirectoryInfo, recursive: bool) =
        if dir.Exists then
            if recursive 
            then printfn $"Recursively deleting {dir.FullName}"
            else printfn $"Deleting {dir.FullName}"
        else
            printfn $"{dir.FullName} does not exist."

    let dir = argument "directory" |> defVal (DirectoryInfo @"c:\default")
    let recursive = option "--recursive" |> defVal false

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setAction action
        addAliases [ "d"; "del" ]
    }


// [<EntryPoint>]
let main argv =
    rootCommand argv {
        description "File System Manager"
        noAction
        addCommands [ listCmd; deleteCmd ]
    }
