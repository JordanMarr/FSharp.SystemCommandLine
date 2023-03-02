module ProgramTask

open FSharp.SystemCommandLine
open System.Threading
open System.Threading.Tasks
open System.CommandLine.Invocation

let app (ctx: InvocationContext, cancel: CancellationToken, words: string array, separator: string) =
    task {
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
    let ctx = Input.Context()
    let cancel = Input.Cancel()
    let words = Input.Option(["--word"; "-w"], [||], "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], ", ", "A character that will separate the joined words.")

    rootCommand argv {
        description "Appends words together"
        inputs (ctx, cancel, words, separator)
        setHandler app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
