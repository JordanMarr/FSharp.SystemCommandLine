## FSharp.SystemCommandLine

The purpose of this library is to:
* provide improved type checks using F# computation expression syntax (mismatches between `inputs` and `setHandler` will be caught at compile time)
* provide helper methods for creating `Option` and `Argument` types (helps to avoid `Option` type name conflict and eliminates need for interface casting)

## Examples

### Simple App

```F#
open System.IO
open FSharp.SystemCommandLine

let app (i: int, b: bool, f: FileInfo) =
    printfn $"The first argument value is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"    
    
[<EntryPoint>]
let main argv = 
    let intArgument = Input.Argument("integer", (fun () -> 53), description = "An integer argument")
    let boolOption = Input.Option("--bool-option", (fun () -> false), "An option whose argument is parsed as a bool")
    let fileOption = Input.Option<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo")

    rootCommand {
        description "System.CommandLine Sample App"
        inputs (intArgument, boolOption, fileOption)
        setHandler app
    }
```

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
