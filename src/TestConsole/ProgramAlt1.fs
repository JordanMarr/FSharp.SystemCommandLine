module ProgramAlt1

open System.CommandLine
open System.IO

type Opt<'T> = System.CommandLine.Option<'T>

let handler (i: int) (b: bool) (f: FileInfo) =
    printfn $"The value for --int-option is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"     

     
//[<EntryPoint>]
let main (argv: string[]) = 
    let intOption = Opt("--int-option", getDefaultValue = (fun () -> 42), description = "An option whose argument is parsed as an int")
    let boolOption = Opt<bool>("--bool-option", "An option whose argument is parsed as a bool") 
    let fileOption = Opt<FileInfo>("--file-option", "An option whose argument is parsed as a FileInfo")

    let cmd = RootCommand()
    cmd.Description <- "My sample app"
    cmd.AddOption intOption
    cmd.AddOption boolOption
    cmd.AddOption fileOption    
    cmd.SetHandler(handler, intOption, boolOption, fileOption)

    cmd.Invoke argv