module ProgramAlt1

open System.IO
open System.CommandLine
open FSharp.SystemCommandLine.Aliases
open System.Threading
open System.Threading.Tasks

let intOption = Opt<int>("int-option", [|"--int-option"|], Description = "An option whose argument is parsed as an int")
intOption.DefaultValueFactory <- fun _ -> 42
let boolOption = Opt<bool>("bool-option", [|"--bool-option"|], Description = "An option whose argument is parsed as a bool")
let fileOption = Opt<FileInfo>("file-option", [|"--file-option"|], Description = "An option whose argument is parsed as a FileInfo")

let action (parseResult: ParseResult) (cancel: CancellationToken) = task {
    let i = parseResult.GetValue intOption
    let b = parseResult.GetValue boolOption
    let f = parseResult.GetValue fileOption
    printfn $"The value for --int-option is: %i{i}"
    printfn $"The value for --bool-option is: %b{b}"
    printfn $"The value for --file-option is: %A{f}"
    return 5
}

let main (argv: string[]) = 
    let cmd = RootCommand()
    cmd.Description <- "My sample app"
    cmd.Add intOption
    cmd.Add boolOption
    cmd.Add fileOption    
    cmd.SetAction(action)

    cmd.Parse(argv).InvokeAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously