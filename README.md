### FSharp.SystemCommandLine (beta5)
[![NuGet version (FSharp.SystemCommandLine)](https://img.shields.io/nuget/v/FSharp.SystemCommandLine.svg?style=flat-square)](https://www.nuget.org/packages/FSharp.SystemCommandLine/)

The purpose of this library is to provide quality of life improvements when using the [`System.CommandLine`](https://github.com/dotnet/command-line-api) API in F#.

_[Click here to view the old beta 4 README](README-beta4.md)_

## Features

* Mismatches between `inputs` and `setAction` handler function parameters are caught at compile time
* `Input.option` helper avoids the need to use the `System.CommandLine.Option` type directly (which conflicts with the F# `Option` type) 
* `Input.optionMaybe` and `Input.argumentMaybe` helpers allow you to use F# `option` types in your handler function.
* `Input.context` helper allows you to pass the `ActionContext` to your action function which is necessary for some operations.
* `Input.validate` helper allows you to validate against parsed value using the F# `Result` type.

## Example

```F#
open System.IO
open FSharp.SystemCommandLine
open Input

let unzip (zipFile: FileInfo, outputDirMaybe: DirectoryInfo option) = 
    // Default to the zip file dir if None
    let outputDir = defaultArg outputDirMaybe zipFile.Directory
    printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}..."
    
[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "Unzips a .zip file"
        inputs (
            argument "zipfile"
            |> desc "The file to unzip"
            |> validateFileExists
            |> validate (fun zipFile ->
                if zipFile.Length <= 500000
                then Ok ()
                else Error $"File cannot be bigger than 500 KB"
            ),

            optionMaybe "--output"
            |> alias "-o"
            |> desc "The output directory"
            |> validateDirectoryExists
        )
        setAction unzip
    }
```

💥WARNING: You must declare `inputs` before `setAction` or else the type checking will not work properly and you will get a build error!💥

```batch
> unzip.exe "c:\test\stuff.zip"
    Result: Unzipping stuff.zip to c:\test
    
> unzip.exe "c:\test\stuff.zip" -o "c:\test\output"
    Result: Unzipping stuff.zip to c:\test\output
```


_Notice that mismatches between the `setAction` and the `inputs` are caught as a compile time error:_
![fs scl demo](https://user-images.githubusercontent.com/1030435/164288239-e0ff595d-cdb2-47f8-9381-50c89aedd481.gif)

## Input API
The new `Input` module contains functions for the underlying System.CommandLine `Option` and `Argument` properties. 
### Inputs
* `context` passes an `ActionContext` containing a `ParseResult` and `CancellationToken` to the action
* `argument` creates a named `Argument<'T>`
* `argumentMaybe` creates a named `Argument<'T option>` that defaults to `None`.
* `option` creates a named `Option<'T>`
* `optionMaybe` creates a named `Option<'T option>` that defaults to `None`.

### Input Properties
* `acceptLegalFilePathsOnly` sets the option or argument to accept only values representing legal file paths.
* `alias` adds an `Alias` to an `Option`
* `aliases` adds one or more aliases to an `Option`
* `desc` adds a description to an `Option` or `Argument`
* `defaultValue` or `def` provides a default value to an `Option` or `Argument`
* `defFactory` assigns a default value factor to an `Option` or `Argument`
* `helpName` adds the name used in help output to describe the option or argument.
* `required` marks an `Option` as required
* `validate` allows you to return a `Result<unit, string>` for the parsed value
* `validateFileExists` ensures that the `FileInfo` exists
* `validateDirectoryExists` ensures that the `DirectoryInfo` exists
* `addValidator` allows you to add a validator to the underlying `Option` or `Argument`
* `acceptOnlyFromAmong` validates the allowed values for an `Option` or `Argument`
* `customParser` allows you to parse the input tokens using a custom parser function.
* `tryParse` allows you to parse the input tokens using a custom parser `Result<'T, string>` function.
* `arity` sets the arity of an `Option` or `Argument`
* `allowMultipleArgumentsPerToken`  allows multiple values for an `Option` or `Argument`. (Defaults to 'false' if not set.)
* `hidden` hides an option or argument from the help output
* `editOption` allows you to pass a function to edit the underlying `Option`
* `editArgument` allows you to pass a function to edit the underlying `Argument`
* `ofOption` allows you to pass a manually created `Option`
* `ofArgument` allows you to pass a manually created `Argument`

### Extensibility
You can easily compose your own custom `Input` functions with `editOption` and `editArgument`.
For example, this is how the existing `alias` and `desc` functions were created:

```fsharp
    let alias (alias: string) (input: ActionInput<'T>) = 
        input
        |> editOption (fun o -> o.Aliases.Add alias)
                
    let desc (description: string) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.Description <- description)
        |> editArgument (fun a -> a.Description <- description)
```
* _Since `alias` can only apply to `Option`, it only calls `editOption`_
* _Since `desc` can apply to both `Option` and `Argument`, you need to use both_

Here is the definition of the built-in `Input.validateFileExists` function which was built with the existing `validate` function:
```fsharp
let validateFileExists (input: ActionInput<System.IO.FileInfo>) = 
    input    
    |> Input.validate (fun file -> 
        if file.Exists then Ok () 
        else Error $"File '{file.FullName}' does not exist."
    )
```

And then use it like this:
```fsharp
let zipFile =
    argument "zipfile"
    |> desc "The file to unzip"
    |> validateFileExists
```

## More Examples

### Simple App that Returns a Status Code

You may optionally return a status code from your handler function.

```F#
open System.IO
open FSharp.SystemCommandLine
open Input

let unzip (zipFile: FileInfo, outputDirMaybe: DirectoryInfo option) = 
    // Default to the zip file dir if None
    let outputDir = defaultArg outputDirMaybe zipFile.Directory

    if zipFile.Exists then
        printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}"
        0 // Program successfully completed.
    else 
        printfn $"File does not exist: {zipFile.FullName}"
        2 // The system cannot find the file specified.
    
[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "Unzips a .zip file"
        inputs (
            argument "zipfile" |> desc "The file to unzip",
            optionMaybe "--output" |> alias "-o" |> desc "The output directory"
        )
        setAction unzip
    }
```


### App with SubCommands

```F#
open System.IO
open FSharp.SystemCommandLine
open Input

// Ex: fsm.exe list "c:\temp"
let listCmd = 
    let action (dir: DirectoryInfo) = 
        if dir.Exists 
        then dir.EnumerateFiles() |> Seq.iter (fun f -> printfn "%s" f.Name)
        else printfn $"{dir.FullName} does not exist."
        
    command "list" {
        description "lists contents of a directory"
        inputs (argument "dir" |> desc "The directory to list")
        setAction action
    }

// Ex: fsm.exe delete "c:\temp" --recursive
let deleteCmd = 
    let action (dir: DirectoryInfo, recursive: bool) = 
        if dir.Exists then 
            if recursive
            then printfn $"Recursively deleting {dir.FullName}"
            else printfn $"Deleting {dir.FullName}"
        else 
            printfn $"{dir.FullName} does not exist."

    let dir = argument "dir" |> desc "The directory to delete"
    let recursive = option "--recursive" |> def false

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setAction action
    }
     
[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "File System Manager"
        noAction
        // if using async task sub commands:
        // noActionAsync
        addCommand listCmd
        addCommand deleteCmd
    }
```

```batch
> fsm.exe list "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine"
    CommandBuilders.fs
    FSharp.SystemCommandLine.fsproj
    pack.cmd
    Types.fs

> fsm.exe delete "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine"
    Deleting c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine

> fsm.exe delete "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine" --recursive
    Recursively deleting c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine
```

### Passing the InvocationContext

You may need to pass the `InvocationContext` to your handler function for the following reasons:
* You need to get `CancellationToken`, `IConsole` or `ParseResult`
* If you have more than 8 inputs, you will need to manually get the parsed values via the `InvocationContext`.

You can pass the `InvocationContext` via the `Input.Context()` method.

```F#
module Program

open FSharp.SystemCommandLine
open Input
open System.Threading
open System.Threading.Tasks
open System.CommandLine.Invocation

let app (ctx: ActionContext, words: string array, separator: string) =
    task {
        let cancel = ctx.CancellationToken
        for i in [1..20] do
            if cancel.IsCancellationRequested then 
                printfn "Cancellation Requested"
                raise (new System.OperationCanceledException())
            else 
                printfn $"{i}"
                do! Task.Delay(1000)

        System.String.Join(separator, words)
        |> printfn "Result: %s"
    }
    
[<EntryPoint>]
let main argv = 
    let ctx = Input.context
    let words = Input.option "--word" |> alias "-w" |> desc "A list of words to be appended"
    let separator = Input.option "--separator" |> alias "-s" |> defaultValue ", " |> desc "A character that will separate the joined words."

    rootCommand argv {
        description "Appends words together"
        inputs (ctx, words, separator)
        setAction app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```

### Example with more than 8 inputs
Currently, a command handler function is limited to accept a tuple with no more than eight inputs.
If you need more, you can pass in the `ActionContext` to your action handler and manually get as many input values as you like (assuming they have been registered in the command builder's `addInputs` operation).

```F#
module Program

open FSharp.SystemCommandLine
open Input

module Parameters = 
    let words = option "--word" |> alias "-w" |> desc "A list of words to be appended"
    let separator = optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."

let app ctx =
    // Manually parse as many parameters as you need
    let words = Parameters.words.GetValue ctx.ParseResult
    let separator = Parameters.separator.GetValue ctx.ParseResult

    // Do work
    let separator = separator |> Option.defaultValue ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "Appends words together"
        inputs Input.context
        setAction app
        addInputs [ Parameters.words; Parameters.separator ]
    }
```

### Example using Microsoft.Extensions.Hosting

This example requires the following nuget packages:

- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Hosting
- Serilog.Extensions.Hosting
- Serilog.Sinks.Console
- Serilog.Sinks.File

```F#
open System
open System.IO
open FSharp.SystemCommandLine
open Input
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Serilog

let buildHost (argv: string[]) =
    Host.CreateDefaultBuilder(argv)
        .ConfigureHostConfiguration(fun configHost ->
            configHost.SetBasePath(Directory.GetCurrentDirectory()) |> ignore
            configHost.AddJsonFile("appsettings.json", optional = false) |> ignore
        )
        .UseSerilog(fun hostingContext configureLogger -> 
            configureLogger
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path = "logs/log.txt", 
                    rollingInterval = RollingInterval.Year
                )
                |> ignore
        )
        .Build()        

let export (logger: ILogger) (connStr: string, outputDir: DirectoryInfo, startDate: DateTime, endDate: DateTime) =
    task {
        logger.Information($"Querying from {StartDate} to {EndDate}", startDate, endDate)            
        // Do export stuff...
    }

[<EntryPoint>]
let main argv =
    let host = buildHost argv
    let logger = host.Services.GetService<ILogger>()
    let cfg = host.Services.GetService<IConfiguration>()

    let connStr =
        Input.option "--connection-string"
        |> Input.alias "-c"
        |> Input.defaultValue (cfg["ConnectionStrings:DB"])
        |> Input.desc "Database connection string"

    let outputDir =
        Input.option "--output-directory"
        |> Input.alias "-o"
        |> Input.defaultValue (DirectoryInfo(cfg["DefaultOutputDirectory"]))
        |> desc "Output directory folder."

    let startDate =
        Input.option "--start-date"
        |> Input.defaultValue (DateTime.Today.AddDays(-7))
        |> desc "Start date (defaults to 1 week ago from today)"
        
    let endDate =
        Input.option "--end-date"
        |> Input.defaultValue DateTime.Today
        |> Input.desc "End date (defaults to today)"

    rootCommand argv {
        description "Data Export"
        inputs (connStr, outputDir, startDate, endDate)
        setAction (export logger)
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```

### Global Options

This example shows to create global options to all child commands. 

```F#
module ProgramNestedSubCommands

open System.IO
open FSharp.SystemCommandLine
open Input

module Global = 
    let enableLogging = option "--enable-logging" |> def false
    let logFile = option "--log-file" |> def (FileInfo @"c:\temp\default.log")

    type Options = { EnableLogging: bool; LogFile: FileInfo }

    let options: ActionInput seq = [ enableLogging; logFile ]

    let bind (ctx: ActionContext) = 
        { EnableLogging = enableLogging.GetValue ctx.ParseResult
          LogFile = logFile.GetValue ctx.ParseResult }

let listCmd =
    let action (ctx: ActionContext, dir: DirectoryInfo) =
        let options = Global.bind ctx
        if options.EnableLogging then 
            printfn $"Logging enabled to {options.LogFile.FullName}"

        if dir.Exists then
            dir.EnumerateFiles()
            |> Seq.iter (fun f -> printfn "%s" f.FullName)
        else
            printfn $"{dir.FullName} does not exist."

    command "list" {
        description "lists contents of a directory"
        inputs (
            Input.context,
            argument "directory" |> def (DirectoryInfo @"c:\default")
        )
        setAction action
        addAlias "ls"
    }

let deleteCmd =
    let action (ctx: ActionContext, dir: DirectoryInfo, recursive: bool) =
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

    let dir = Input.argument "directory" |> def (DirectoryInfo @"c:\default")
    let recursive = Input.option "--recursive" |> def false

    command "delete" {
        description "deletes a directory"
        inputs (Input.context, dir, recursive)
        setAction action
        addAlias "del"
    }

let ioCmd = 
    command "io" {
        description "Contains IO related subcommands."
        noAction
        addCommands [ deleteCmd; listCmd ]
    }

[<EntryPoint>]
let main (argv: string array) =
    let cfg = 
        commandLineConfiguration {
            description "Sample app for System.CommandLine"
            noAction
            addGlobalOptions Global.options
            addCommand ioCmd
        }

    let parseResult = cfg.Parse(argv)

    // Get global option value from the parseResult
    let loggingEnabled = Global.enableLogging.GetValue parseResult
    printfn $"ROOT: Logging enabled: {loggingEnabled}"

    parseResult.Invoke()
```

### Database Migrations Example

This real-life example for running database migrations demonstrates the following features:
* Uses Microsoft.Extensions.Hosting.
* Uses async/task commands.
* Passes the `ILogger` dependency to the commands.
* Shows help if no command is passed.

```F#
module Program

open EvolveDb
open System.Data.SqlClient
open System.IO
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Serilog
open EvolveDb.Configuration
open FSharp.SystemCommandLine
open Input
open System.CommandLine.Invocation
open System.CommandLine.Help

let buildHost (argv: string[]) =
    Host.CreateDefaultBuilder(argv)
        .ConfigureHostConfiguration(fun configHost ->
            configHost.SetBasePath(Directory.GetCurrentDirectory()) |> ignore
            configHost.AddJsonFile("appsettings.json", optional = false) |> ignore
        )
        .UseSerilog(fun hostingContext configureLogger ->
            configureLogger
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path = "logs/log.txt", 
                    rollingInterval = RollingInterval.Year
                )
                |> ignore
        )
        .Build()

let repairCmd (logger: ILogger) = 
    let action (env: string) =
        task {
            logger.Information($"Environment: {env}")
            logger.Information("Starting EvolveDb Repair (correcting checksums).")
            let! connStr = KeyVault.getConnectionString env
            use conn = new SqlConnection(connStr)
            let evolve = Evolve(conn, fun msg -> printfn "%s" msg) 
            evolve.TransactionMode <- TransactionKind.CommitAll
            evolve.Locations <- [| "Scripts" |]
            evolve.IsEraseDisabled <- true
            evolve.MetadataTableName <- "_EvolveChangelog"
            evolve.Repair()
        }

    command "repair" {
        description "Corrects checksums in the database."
        inputs (argument "env" |> desc "The keyvault environment: [dev, beta, prod].")
        setAction action
    }

let migrateCmd (logger: ILogger) =
    let action (env: string) =
        task {
            logger.Information($"Environment: {env}")
            logger.Information("Starting EvolveDb Migrate.")
            let! connStr = KeyVault.getConnectionString env
            use conn = new SqlConnection(connStr)
            let evolve = Evolve(conn, fun msg -> printfn "%s" msg) 
            evolve.TransactionMode <- TransactionKind.CommitAll
            evolve.Locations <- [| "Scripts" |]
            evolve.IsEraseDisabled <- true
            evolve.MetadataTableName <- "_EvolveChangelog"
            evolve.Migrate()
        }

    command "migrate" {
        description "Migrates the database."
        inputs (argument "env" |> desc "The keyvault environment: [dev, beta, prod].")
        setAction action
    }

[<EntryPoint>]
let main argv =
    let host = buildHost argv
    let logger = host.Services.GetService<ILogger>()

    rootCommand argv {
        description "Database Migrations"
        inputs Input.context    // Required input for helpAction
        helpAction              // Show --help if no sub-command is called
        addCommand (repairCmd logger)
        addCommand (migrateCmd logger)
    }

```

### Creating a Root Command Parser

If you want to manually invoke your root command, use the `rootCommandParser` CE (because the `rootCommand` CE is auto-executing).

```F#
open FSharp.SystemCommandLine
open Input
open System.CommandLine.Parsing

let app (words: string array, separator: string option) =
    let separator = defaultArg separator ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
[<EntryPoint>]
let main argv = 
    let words = option "--word" |> alias -w" |> desc "A list of words to be appended"
    let separator = optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."

    let cfg = 
        commandLineConfiguration {
            description "Appends words together"
            inputs (words, separator)
            setAction app
        }

    let parseResult = cfg.Parse(argv)
    parseResult.Invoke()
```

### Showing Help as the Default
A common design is to show help information if no commands have been passed:

```F#
open System.CommandLine.Invocation
open System.CommandLine.Help
open FSharp.SystemCommandLine
open Input

let helloCmd = 
    let action name = printfn $"Hello, %s{name}."
    let name = argument "Name"
    command "hello" {
        description "Says hello."
        inputs name
        setAction action
    }

[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "Says hello or shows help by default."
        inputs Input.context
        helpAction
        addCommand helloCmd
    }
```

## Command Line Configuration

System.CommandLine has a `CommandLineConfiguration` that allows the user to customize various behaviors.

FSharp.SystemCommandLine uses the defaults from `CommandLineConfiguration`, but you can override them via the `configure` custom operation which gives you access to the `CommandLineConfiguration`. 

For example, the default behavior intercepts input strings that start with a "@" character via the "TryReplaceToken" feature. This will cause an issue if you need to accept input that starts with "@". Fortunately, you can disable this via `usePipeline`:

```F#
module TokenReplacerExample

open FSharp.SystemCommandLine
open Input

let app (package: string) =
    if package.StartsWith("@") then
        printfn $"{package}"
        0
    else
        eprintfn "The package name does not start with a leading @"
        1

[<EntryPoint>]
let main argv =

    // The package option needs to accept strings that start with "@" symbol.
    // For example, "--package @shoelace-style/shoelace".
    // To accomplish this, we will need to modify the configuration below.
    let package = option "--package" |> alias "-p" |> desc "A package name that may have a leading '@' character."

    rootCommand argv {
        description "Can be called with a leading '@' package"
        configure (fun cfg -> 
            // Override default token replacer to ignore `@` processing
            cfg.ResponseFileTokenReplacer <- null
        )
        inputs package
        setAction app
    }
```
