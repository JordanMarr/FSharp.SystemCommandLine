module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open Input
open System.Threading

module Global = 
    let enableLogging = option "--enable-logging" |> def false
    let logFile = option "--log-file" |> def (FileInfo @"c:\temp\default.log")

    type Options = { EnableLogging: bool; LogFile: FileInfo }
    let options: ActionInput seq = [ enableLogging; logFile ] 

let listCmd =
    // NOTE: A global option is directly passed as input in this example.
    let action (dir: DirectoryInfo, enableLogging: bool) = 
        if enableLogging then 
            printfn $"Logging enabled."

        if dir.Exists then
            dir.EnumerateFiles()
            |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else
            printfn $"{dir.FullName} does not exist."

    let dir = argument "directory" |> validateDirectoryExists  // |> def (DirectoryInfo @"c:\default")

    command "list" {
        description "lists contents of a directory"
        inputs (dir, Global.enableLogging)
        setAction action
        addAlias "ls"
    }

let deleteCmd =
    // NOTE: Global options are accessed via the context in this example.
    let action (ctx, dir: DirectoryInfo, recursive: bool) =
        let enableLogging = Global.enableLogging.GetValue ctx.ParseResult
        let logFile = Global.logFile.GetValue ctx.ParseResult

        if enableLogging then 
            printfn $"Logging enabled to {logFile.FullName}."
        
        if recursive 
        then printfn $"Recursively deleting {dir.FullName}"
        else printfn $"Deleting {dir.FullName}"

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
    |> Async.AwaitTask
    |> Async.RunSynchronously

let run () = 
    "io list \"c:/data/\" --enable-logging" |> Utils.args |> main
    //"io list \"--enable-logging" |> Utils.args |> main
    