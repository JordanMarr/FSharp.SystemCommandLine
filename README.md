### FSharp.SystemCommandLine [![NuGet version (FSharp.SystemCommandLine)](https://img.shields.io/nuget/v/FSharp.SystemCommandLine.svg?style=flat-square)](https://www.nuget.org/packages/FSharp.SystemCommandLine/)

The purpose of this library is to improve type safety when using the `System.CommandLine` API in F# by utilizing computation expression syntax.



## Features

### Improved type safety
* Mismatches between `inputs` and `setHandler` are caught at compile time

### Provide helper methods via the `Input` class for creating `Option` and `Argument` types 
* Avoids initializing the `Option` type directly (which conflicts with the F# `Option` type) 
* Eliminates the need to manually cast inputs to the `IValueDescriptor` interface

## Examples

### Simple App

```F#
open FSharp.SystemCommandLine
open System.IO

let unzip (zipFile: FileInfo, outputDir: DirectoryInfo) = 
    // Default to the zip file dir if null
    let outputDir = 
        outputDir 
        |> Option.ofObj 
        |> Option.defaultValue zipFile.Directory

    if zipFile.Exists
    then printfn $"Unzipping {zipFile.Name} to {outputDir.FullName}"
    else printfn $"File does not exist: {zipFile.FullName}"
    
[<EntryPoint>]
let main argv = 
    let zipFile = Input.Argument<FileInfo>("The file to unzip")    
    let outputDir = Input.Option<DirectoryInfo>("-o", (fun () -> null), "The output directory")

    rootCommand argv {
        description "Unzips a .zip file"
        inputs (zipFile, outputDir)
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


### Async App with a Dependency and Pipeline Override

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
    let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], (fun () -> ", "), "A character that will separate the joined words.")

    // Initialize app dependencies
    let svc = WordService()

    rootCommand argv {
        description "Appends words together"
        inputs (words, separator)
        usePipeline (fun builder -> 
            builder.UseTypoCorrections(3)   // Override pipeline
        )
        setHandler (app svc)                // Partially apply app dependencies
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```

### App with SubCommands

```F#
open System.IO
open FSharp.SystemCommandLine

let listCmd = 
    let handler (dir: DirectoryInfo) = 
        if dir.Exists 
        then dir.EnumerateFiles() |> Seq.iter (fun f -> printfn "%s" f.Name)
        else printfn $"{dir.FullName} does not exist."
        
    let dir = Input.Argument(fun () -> DirectoryInfo("c:\fake dir"))

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

    let dir = Input.Argument(fun () -> DirectoryInfo("c:\fake dir"))
    let recursive = Input.Option("--recursive", getDefaultValue = (fun () -> false))

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
