module ProgramNoArgs

open System.IO
open System.CommandLine.Binding
open FSharp.SystemCommandLine
    
let app () = 
    printfn "Do stuff"

//[<EntryPoint>]
let main argv = 
    let intOption = Opt("--int-option", getDefaultValue = (fun () -> 42), description = "An option whose argument is parsed as an int") :> IValueDescriptor<_>
    let boolOption = Opt<bool>("--bool-option", "An option whose argument is parsed as a bool") :> IValueDescriptor<_>
    let fileOption = Opt<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo") :> IValueDescriptor<_>

    rootCommand {
        description "My sample app"
        setHandler app
    }
    
        
