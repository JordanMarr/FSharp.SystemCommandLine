﻿module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open System.CommandLine.Invocation
open System.CommandLine.Parsing

module Global = 
    let enableLogging = Input.Option<bool>("--enable-logging", false)
    let logFile = Input.Option<FileInfo>("--log-file", FileInfo @"c:\temp\default.log")

    type Options = { EnableLogging: bool; LogFile: FileInfo }

    let options: HandlerInput seq = [ enableLogging; logFile ] 

    let bind (ctx: InvocationContext) = 
        { EnableLogging = enableLogging.GetValue ctx
          LogFile = logFile.GetValue ctx }

let listCmd =
    let handler (ctx: InvocationContext, dir: DirectoryInfo) =
        let options = Global.bind ctx
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
        let options = Global.bind ctx
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
    let ctx = Input.Context()
    
    let parser = 
        rootCommandParser {
            description "Sample app for System.CommandLine"
            setHandler id
            addGlobalOptions Global.options
            addCommand ioCmd
        }

    let parseResult = parser.Parse(argv)

    let loggingEnabled = Global.enableLogging.GetValue parseResult
    printfn $"ROOT: Logging enabled: {loggingEnabled}"

    parseResult.Invoke()
    