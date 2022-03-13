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
