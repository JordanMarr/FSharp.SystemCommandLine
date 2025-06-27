module ProgramSubCommand

open System.IO
open FSharp.SystemCommandLine
open Input

let listCmd =
    let action (dir: DirectoryInfo) =
        dir.EnumerateFiles()
        |> Seq.iter (fun f -> printfn "%s" f.FullName)

    command "list" {
        description "lists contents of a directory"
        inputs (
            argument "directory" 
            |> defaultValue (DirectoryInfo @"c:\default")
            |> validateDirectoryExists
        )
        setAction action
        addAlias "ls"
    }

let deleteCmd =
    let action (dir: DirectoryInfo, recursive: bool) =
        if recursive 
        then printfn $"Recursively deleting {dir.FullName}"
        else printfn $"Deleting {dir.FullName}"

    command "delete" {
        description "deletes a directory"
        inputs (
            argument "directory" 
            |> defaultValue (DirectoryInfo @"c:\default")
            |> validateDirectoryExists,

            option "--recursive" 
            |> defaultValue false
        )
        setAction action
        addAliases [ "d"; "del" ]
    }

let main argv =
    rootCommand argv {
        description "File System Manager"
        noAction
        addCommands [ listCmd; deleteCmd ]
    }
