module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open System.CommandLine.Invocation

module GlobalOptions = 
    let enableLogging = Input.Option<bool>("--enable-logging", false)
    let logFile = Input.Option<FileInfo>("--log-file", FileInfo @"c:\temp\default.log")

    type GlobalOptions = { EnableLogging: bool; LogFile: FileInfo }

    let all: HandlerInput seq = [ enableLogging; logFile ] 

    let bind (ctx: InvocationContext) = 
        { EnableLogging = enableLogging.GetValue ctx
          LogFile = logFile.GetValue ctx }

let listCmd =
    let handler (ctx: InvocationContext, dir: DirectoryInfo) =
        let options = GlobalOptions.bind ctx
        if options.EnableLogging then 
            printfn $"Logging enabled to {options.LogFile.FullName}"

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
    let handler (ctx: InvocationContext, dir: DirectoryInfo, recursive: bool) =
        let options = GlobalOptions.bind ctx
        if options.EnableLogging then 
            printfn $"Logging enabled to {options.LogFile.FullName}"
        
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
        inputs (Input.Context(), dir, recursive)
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
        addGlobalOptions GlobalOptions.all
        addCommand ioCmd
    }
