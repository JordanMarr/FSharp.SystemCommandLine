module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open System.CommandLine.Invocation

/// A global option to enable logging.
let enableLogging = Input.Option<bool>("--enable-logging", false)

let listCmd =
    let handler (ctx: InvocationContext, dir: DirectoryInfo) =
        let enableLogging = enableLogging.GetValue(ctx)
        printfn "enableLogging: %b" enableLogging
        if dir.Exists then
            dir.EnumerateFiles()
            |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else
            printfn $"{dir.FullName} does not exist."

    let dir = Input.Argument("directory", DirectoryInfo(@"c:\default"))

    command "list" {
        description "lists contents of a directory"
        inputs (Input.Context(), dir)
        setHandler handler
        addAlias "ls"
    }

let deleteCmd =
    let handler (dir: DirectoryInfo, recursive: bool) =
        if dir.Exists then
            if recursive then
                printfn $"Recursively deleting {dir.FullName}"
            else
                printfn $"Deleting {dir.FullName}"
        else
            printfn $"{dir.FullName} does not exist."

    let dir = Input.Argument("directory", DirectoryInfo(@"c:\default"))
    let recursive = Input.Option("--recursive", false)

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setHandler handler
        addAlias "del"
    }

let ioCmd = 
    command "io" {
        description "Contains IO related subcommands."
        setHandler id
        addCommands [ deleteCmd; listCmd ]
    }


//[<EntryPoint>]
let main argv =
    rootCommand argv {
        description "Sample app for System.CommandLine"
        setHandler id
        addGlobalOption enableLogging
        addCommand ioCmd
    }
