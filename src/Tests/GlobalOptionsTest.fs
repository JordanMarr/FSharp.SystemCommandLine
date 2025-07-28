module GlobalOptionsTest

open System.IO
open FSharp.SystemCommandLine
open Input
open NUnit.Framework
open Swensen.Unquote
open Utils

module Global = 
    let enableLogging = option "--enable-logging" |> recursive
    let logFile = option "--log-file" |> def (FileInfo @"c:\temp\default.log") |> recursive

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

[<Test>]
let ``01 - Global options with nested commands`` () = 
    testRootCommand "io list \"c:/data/\" --enable-logging" {
        description "Contains IO related subcommands."
        noAction
        addInputs Global.options
        addCommand ioCmd
    } =! 0
