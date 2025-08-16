module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open Input
open System.Threading

module Global = 
    let enableLogging = option "--enable-logging" |> recursive |> def false
    let logFile = option "--log-file" |> recursive |> def (FileInfo @"c:\temp\default.log")

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

open ManualInvocation

let main (argv: string[]) =
    let rootCmd = 
        rootCommand {
            description "Sample app for System.CommandLine"
            noAction
            addInputs Global.options
            addCommand ioCmd
        }

    let parseResult = rootCmd.Parse(argv)

    let loggingEnabled = Global.enableLogging.GetValue parseResult
    printfn $"ROOT: Logging enabled: {loggingEnabled}"

    use cts = new CancellationTokenSource() 
    parseResult.InvokeAsync(cancellationToken = cts.Token)

let run () = 
    "io list \"c:/data/\" --enable-logging" |> Utils.args |> main
    