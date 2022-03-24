### FSharp.SystemCommandLine 
[![NuGet version (FSharp.SystemCommandLine)](https://img.shields.io/nuget/v/FSharp.SystemCommandLine.svg?style=flat-square)](https://www.nuget.org/packages/FSharp.SystemCommandLine/)

The purpose of this library is to improve type safety when using the `System.CommandLine` API in F# by utilizing computation expression syntax.



## Features

### Improved type safety
* Mismatches between `inputs` and `setHandler` are caught at compile time

### Provide helper methods via the `Input` class for creating `Option` and `Argument` types 
* Avoids initializing the `Option` type directly (which conflicts with the F# `Option` type) 
* `FSharp.SystemCommandLine.Aliases` module contains `Opt` and `Arg` aliases (as an alternative to using the `Input` helper class)

### Support for F# `Option` type
* `Input.OptionMaybe` and `Input.ArgumentMaybe` allow you to use F# `option` types in your handler function.

## Examples

### Simple App

```F#
open FSharp.SystemCommandLine
open System.IO

let unzip (zipFile: FileInfo, outputDirMaybe: DirectoryInfo option) = 
    // Default to the zip file dir if null
    let outputDir = outputDirMaybe |> Option.defaultValue zipFile.Directory

    if zipFile.Exists
    then printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}"
    else printfn $"File does not exist: {zipFile.FullName}"
    
[<EntryPoint>]
let main argv = 
    let zipFile = Input.Argument("The file to unzip")    
    let outputDirMaybe = Input.OptionMaybe(["--output"; "-o"], "The output directory")

    rootCommand argv {
        description "Unzips a .zip file"
        inputs (zipFile, outputDirMaybe)
        setHandler unzip
    }
```

```batch
> unzip.exe "c:\test\stuff.zip"
    Result: Unzipping stuff.zip to c:\test
    
> unzip.exe "c:\test\stuff.zip" -o "c:\test\output"
    Result: Unzipping stuff.zip to c:\test\output
```

Notice that mismatches between the `setHandler` and the `inputs` are caught as a compile time error:
![cli safety](https://user-images.githubusercontent.com/1030435/158190730-b1ae0bbf-825b-48c4-b267-05a1853de4d9.gif)


### App with SubCommands

```F#
open System.IO
open FSharp.SystemCommandLine

let listCmd = 
    let handler (dir: DirectoryInfo) = 
        if dir.Exists 
        then dir.EnumerateFiles() |> Seq.iter (fun f -> printfn "%s" f.Name)
        else printfn $"{dir.FullName} does not exist."
        
    let dir = Input.Argument("dir", DirectoryInfo(@"c:\fake dir"))

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setHandler handler
    }

let deleteCmd = 
    let handler (dir: DirectoryInfo, recursive: bool) = 
        if dir.Exists then 
            if recursive
            then printfn $"Recursively deleting {dir.FullName}"
            else printfn $"Deleting {dir.FullName}"
        else 
            printfn $"{dir.FullName} does not exist."

    let dir = Input.Argument("dir", DirectoryInfo(@"c:\fake dir")
    let recursive = Input.Option("--recursive", false)

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
        setCommand listCmd
        setCommand deleteCmd
    }
```

```batch
> TestConsole list "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine"
    CommandBuilders.fs
    FSharp.SystemCommandLine.fsproj
    pack.cmd
    Types.fs

> TestConsole delete "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine"
    Deleting c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine

> TestConsole delete "c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine" --recursive
    Recursively deleting c:\_github\FSharp.SystemCommandLine\src\FSharp.SystemCommandLine
```


### Async App with a Partially Applied Dependency

```F#
open FSharp.SystemCommandLine
open System.CommandLine.Builder
open System.Threading.Tasks

type WordService() = 
    member _.Join(separator: string, words: string array) = 
        task {
            do! Task.Delay(1000)
            return System.String.Join(separator, words)
        }

let app (svc: WordService) (words: string array, separator: string) =
    task {
        let! result = svc.Join(separator, words)
        result |> printfn "Result: %s"
    }
    
[<EntryPoint>]
let main argv = 
    let words = Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], ", ", "A character that will separate the joined words.")

    // Initialize app dependencies
    let svc = WordService()

    rootCommand argv {
        description "Appends words together"
        inputs (words, separator)
        usePipeline (fun builder -> 
            CommandLineBuilder()            // Pipeline is initialized with .UseDefaults() by default,
                .UseTypoCorrections(3)      // but you can override it here if needed.
        )
        setHandler (app svc)                // Partially apply app dependencies
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```
