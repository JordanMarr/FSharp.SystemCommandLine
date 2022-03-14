# FSharp.SystemCommandLine ![NuGet version (FSharp.SystemCommandLine)](https://img.shields.io/nuget/v/FSharp.SystemCommandLine.svg?style=flat-square)

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

let app (words: string array, separator: string) =
    System.String.Join(separator, words)
    |> printfn "Result: %s"
    
[<EntryPoint>]
let main argv = 
    let oWords = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let oSeparator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")

    rootCommand {
        description "Appends words together"
        inputs (oWords, oSeparator)
        setHandler app
    }        
```

```batch
> .\TestConsole --word "hello"
Result: hello
> .\TestConsole --word "hello" -w "world"
Result: hello,world
> .\TestConsole --word "hello" -w "world" -s "***"
Result: hello***world
```

Notice that mismatches between the `setHandler` and the `inputs` are caught as a compile time error:
![cli safety](https://user-images.githubusercontent.com/1030435/158190730-b1ae0bbf-825b-48c4-b267-05a1853de4d9.gif)


### Simple Async App

```F#
open System.IO
open FSharp.SystemCommandLine

let app (i: int, b: bool, f: FileInfo) =
    task {
        printfn $"The value for --int-option is: %i{i}"
        printfn $"The value for --bool-option is: %b{b}"
        printfn $"The value for --file-option is: %A{f}"    
    }
    
[<EntryPoint>]
let main argv = 
    let intOption = Input.Option("--int-option", getDefaultValue = (fun () -> 42), description = "An option whose argument is parsed as an int")
    let boolOption = Input.Option<bool>("--bool-option", "An option whose argument is parsed as a bool")
    let fileOption = Input.Option<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo")

    rootCommand {
        description "My sample app"
        inputs (intOption, boolOption, fileOption)
        setHandler app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
```

### App with a SubCommand

```F#
open FSharp.SystemCommandLine

let listCmd = 
    let handler (path: string) = 
        printfn $"The path is {path}."
        
    let oPath = Input.Option("--path", getDefaultValue = (fun () -> "/"), description = "The path to list")

    command "list" {
        description "lists contents of a folder"
        inputs (oPath)
        setHandler handler
    }

[<EntryPoint>]
let main argv = 
    rootCommand {
        description "My sample app"
        setHandler id
        setCommand listCmd
    }
```
