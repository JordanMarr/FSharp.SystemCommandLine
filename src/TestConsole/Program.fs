module Program

open System.IO
open FSharp.SystemCommandLine

let app (i: int, b: bool, f: FileInfo) =
    printfn $"The value for --int-option is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"    
    
[<EntryPoint>]
let main argv = 
    let intOption = Opt("--int-option", getDefaultValue = (fun () -> 42), description = "An option whose argument is parsed as an int")
    let boolOption = Opt<bool>("--bool-option", "An option whose argument is parsed as a bool") 
    let fileOption = Opt<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo")

    rootCommand {
        description "System.CommandLine Sample App"
        options (intOption, boolOption, fileOption)
        setHandler app
    }
        
