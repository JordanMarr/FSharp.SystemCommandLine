module Program

open System.IO
open System.CommandLine.Binding
open FSharp.SystemCommandLine

let app (i: int, b: bool, f: FileInfo) =
    printfn $"The first argument value is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"    
    
[<EntryPoint>]
let main argv = 
    let intArgument = Input.Argument("integer", (fun () -> 53), description = "An integer argument")
    let boolOption = Input.Option<bool>("--bool-option", "An option whose argument is parsed as a bool")
    let fileOption = Input.Option<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo") :> IValueDescriptor<_>

    rootCommand {
        description "System.CommandLine Sample App"
        inputs (intArgument, boolOption, fileOption)
        setHandler app
    }
        
