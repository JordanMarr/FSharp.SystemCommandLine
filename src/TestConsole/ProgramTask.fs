module ProgramTask

open FSharp.SystemCommandLine
open System.Threading.Tasks
open System.CommandLine

let app (res: ParseResult, words: string array, separator: string) =
    task {
        let cancel = res.GetCancellationToken()

        for i in [1..20] do
            if cancel.IsCancellationRequested then 
                printfn "Cancellation Requested!"
                raise (new System.OperationCanceledException())
            else 
                printfn $"{i}"
                do! Task.Delay(1000)

        System.String.Join(separator, words)
        |> printfn "Result: %s"
    }
    
//[<EntryPoint>]
let main argv = 
    let parseResult = Input.ParseResult()
    let words = Input.Option("word", ["--word"; "-w"], [||], "A list of words to be appended")
    let separator = Input.Option("separator", ["--separator"; "-s"], ", ", "A character that will separate the joined words.")

    rootCommand argv {
        description "Appends words together"
        inputs (parseResult, words, separator)
        setHandler app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
