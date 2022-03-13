module Program

open System.IO
open System.CommandLine.Binding
open FSharp.SystemCommandLine

let app (i: int, b: bool, f: FileInfo) =
    printfn $"The value for --int-option is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"    
    
[<EntryPoint>]
let main argv = 
    let intOption = Opt("--int-option", getDefaultValue = (fun () -> 42), description = "An option whose argument is parsed as an int") :> IValueDescriptor<_>
    let boolOption = Opt<bool>("--bool-option", "An option whose argument is parsed as a bool") :> IValueDescriptor<_>
    let fileOption = Opt<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo") :> IValueDescriptor<_>

    rootCommand {
        description "System.CommandLine Sample App"
        inputs (intOption, boolOption, fileOption)
        setHandler app
    }
        
