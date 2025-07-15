module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open Input

module Global = 
    let enableLogging = option "--enable-logging" |> def false
    let logFile = option "--log-file" |> def (FileInfo @"c:\temp\default.log")

    type Options = { EnableLogging: bool; LogFile: FileInfo }

    let options: ActionInput seq = [ enableLogging; logFile ] 

    let bind parseResult = 
        { EnableLogging = enableLogging.GetValue parseResult
          LogFile = logFile.GetValue parseResult }

let listCmd =
    let action (ctx, dir: DirectoryInfo) =
        let options = Global.bind ctx.ParseResult
        if options.EnableLogging then 
            printfn $"Logging enabled to {options.LogFile.FullName}"

        if dir.Exists then
            dir.EnumerateFiles()
            |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else
            printfn $"{dir.FullName} does not exist."

    let dir = argument "directory" |> def (DirectoryInfo @"c:\default")

    command "list" {
        description "lists contents of a directory"
        inputs (context, dir)
        setAction action
        addAlias "ls"
    }

let deleteCmd =
    let action (ctx, dir: DirectoryInfo, recursive: bool) =
        let options = Global.bind ctx.ParseResult
        if options.EnableLogging then 
            printfn $"Logging enabled to {options.LogFile.FullName}"
        
        if dir.Exists then
            if recursive then
                printfn $"Recursively deleting {dir.FullName}"
            else
                printfn $"Deleting {dir.FullName}"
        else
            printfn $"{dir.FullName} does not exist."

    let dir = argument "directory" |> def (DirectoryInfo @"c:\default")
    let recursive = option "--recursive" |> def false

    command "delete" {
        description "deletes a directory"
        inputs (context, dir, recursive)
        setAction action
        addAlias "del"
    }

let ioCmd = 
    command "io" {
        description "Contains IO related subcommands."
        noAction
        addCommands [ deleteCmd; listCmd ]
    }

let main (argv: string[]) =
    let cfg = 
        commandLineConfiguration {
            description "Sample app for System.CommandLine"
            noAction
            addGlobalOptions Global.options
            addCommand ioCmd
        }

    //let parseResult = CommandLineParser.Parse(cfg.RootCommand, argv)
    let parseResult = cfg.Parse(argv)

    let loggingEnabled = Global.enableLogging.GetValue parseResult
    printfn $"ROOT: Logging enabled: {loggingEnabled}"

    parseResult.Invoke()

let run () = 
    "io list \"c:/data/\" --enable-logging" |> Utils.args |> main
    