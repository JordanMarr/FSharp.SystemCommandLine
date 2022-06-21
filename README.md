### FSharp.SystemCommandLine 
[![NuGet version (FSharp.SystemCommandLine)](https://img.shields.io/nuget/v/FSharp.SystemCommandLine.svg?style=flat-square)](https://www.nuget.org/packages/FSharp.SystemCommandLine/)

The purpose of this library is to provide quality of life improvements when using the [`System.CommandLine`](https://github.com/dotnet/command-line-api) API in F#.

## Features

* Mismatches between `inputs` and `setHandler` function parameters are caught at compile time
* `Input.Option` helper method avoids the need to use the `System.CommandLine.Option` type directly (which conflicts with the F# `Option` type) 
* `Input.OptionMaybe` and `Input.ArgumentMaybe` methods allow you to use F# `option` types in your handler function.
* `Input.Context` method allows you to pass the `System.CommandLine.Invocation.InvocationContext` to your handler function.

## Examples

### Simple App

```F#
open FSharp.SystemCommandLine
open System.IO

let unzip (zipFile: FileInfo, outputDirMaybe: DirectoryInfo option) = 
    // Default to the zip file dir if None
    let outputDir = outputDirMaybe |> Option.defaultValue zipFile.Directory

    if zipFile.Exists
    then printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}"
    else printfn $"File does not exist: {zipFile.FullName}"
    
[<EntryPoint>]
let main argv = 
    let zipFile = Input.Argument<FileInfo>("The file to unzip")    
    let outputDirMaybe = Input.OptionMaybe<DirectoryInfo>(["--output"; "-o"], "The output directory")

    rootCommand argv {
        description "Unzips a .zip file"
        inputs (zipFile, outputDirMaybe) // must be set before setHandler
        setHandler unzip
    }
```

💥WARNING: You must declare `inputs` before `setHandler` or else the type checking will not work properly and you will get a build error!💥

```batch
> unzip.exe "c:\test\stuff.zip"
    Result: Unzipping stuff.zip to c:\test
    
> unzip.exe "c:\test\stuff.zip" -o "c:\test\output"
    Result: Unzipping stuff.zip to c:\test\output
```


_Notice that mismatches between the `setHandler` and the `inputs` are caught as a compile time error:_
![fs scl demo](https://user-images.githubusercontent.com/1030435/164288239-e0ff595d-cdb2-47f8-9381-50c89aedd481.gif)


### Simple App that Returns a Status Code

You may optionally return a status code from your handler function.

```F#
open FSharp.SystemCommandLine
open System.IO

let unzip (zipFile: FileInfo, outputDirMaybe: DirectoryInfo option) = 
    // Default to the zip file dir if None
    let outputDir = outputDirMaybe |> Option.defaultValue zipFile.Directory

    if zipFile.Exists then
        printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}"
        0 // Program successfully completed.
    else 
        printfn $"File does not exist: {zipFile.FullName}"
        2 // The system cannot find the file specified.
    
[<EntryPoint>]
let main argv = 
    let zipFile = Input.Argument<FileInfo>("The file to unzip")    
    let outputDirMaybe = Input.OptionMaybe<DirectoryInfo>(["--output"; "-o"], "The output directory")

    rootCommand argv {
        description "Unzips a .zip file"
        inputs (zipFile, outputDirMaybe)
        setHandler unzip
    }
```


### App with SubCommands

```F#
open System.IO
open FSharp.SystemCommandLine

// Ex: fsm.exe list "c:\temp"
let listCmd = 
    let handler (dir: DirectoryInfo) = 
        if dir.Exists 
        then dir.EnumerateFiles() |> Seq.iter (fun f -> printfn "%s" f.Name)
        else printfn $"{dir.FullName} does not exist."
        
    let dir = Input.Argument<DirectoryInfo>("dir", "The directory to list")

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setHandler handler
    }

// Ex: fsm.exe delete "c:\temp" --recursive
let deleteCmd = 
    let handler (dir: DirectoryInfo, recursive: bool) = 
        if dir.Exists then 
            if recursive
            then printfn $"Recursively deleting {dir.FullName}"
            else printfn $"Deleting {dir.FullName}"
        else 
            printfn $"{dir.FullName} does not exist."

    let dir = Input.Argument<DirectoryInfo>("dir", "The directory to delete")
    let recursive = Input.Option<bool>("--recursive", false)

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setHandler handler
    }
        

[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "File System Manager"
        setHandler id
        // if using async task sub commands, setHandler to `Task.FromResult`
        // setHandler Task.FromResult         
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
open System.Threading
open System.Threading.Tasks
open System.CommandLine.Invocation

let app (ctx: InvocationContext, words: string array, separator: string) =
    task {
        let cancel = ctx.GetCancellationToken()
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
    let ctx = Input.Context()
    let words = Input.Option<string array>(["--word"; "-w"], [||], "A list of words to be appended")
    let separator = Input.Option<string>(["--separator"; "-s"], ", ", "A character that will separate the joined words.")

    rootCommand argv {
        description "Appends words together"
        inputs (ctx, words, separator)
        setHandler app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
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

let exportHandler (logger: ILogger) (connStr: string, outputDir: DirectoryInfo, startDate: DateTime, endDate: DateTime) =
    task {
        logger.Information($"Querying from {StartDate} to {EndDate}", startDate, endDate)            
        // Do export stuff...
    }

[<EntryPoint>]
let main argv =
    let host = buildHost argv
    let logger = host.Services.GetService<ILogger<_>>()
    let cfg = host.Services.GetService<IConfiguration>()

    let connStr = Input.Option<string>(
        aliases = ["-c"; "--connection-string"], 
        defaultValue = cfg["ConnectionStrings:DB"],
        description = "Database connection string")

    let outputDir = Input.Option<DirectoryInfo>(
        aliases = ["-o";"--output-directory"], 
        defaultValue = DirectoryInfo(cfg["DefaultOutputDirectory"]), 
        description = "Output directory folder.")

    let startDate = Input.Option<DateTime>(
        name = "--start-date", 
        defaultValue = DateTime.Today.AddDays(-7), 
        description = "Start date (defaults to 1 week ago from today)")
        
    let endDate = Input.Option<DateTime>(
        name = "--end-date", 
        defaultValue = DateTime.Today, 
        description = "End date (defaults to today)")

    rootCommand argv {
        description "Data Export"
        inputs (connStr, outputDir, startDate, endDate)
        setHandler (exportHandler logger)
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```

### Creating a Root Command Parser

If you want to manually invoke your root command, use the `rootCommandParser` CE (because the `rootCommand` CE is auto-executing).

```F#
open FSharp.SystemCommandLine
open System.CommandLine.Parsing

let app (words: string array, separator: string option) =
    let separator = separator |> Option.defaultValue ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
[<EntryPoint>]
let main argv = 
    let words = Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended")
    let separator = Input.OptionMaybe(["--separator"; "-s"], "A character that will separate the joined words.")

    let parser = 
        rootCommandParser {
            description "Appends words together"
            inputs (words, separator)
            setHandler app
        }

    parser.Parse(argv).Invoke()
```
