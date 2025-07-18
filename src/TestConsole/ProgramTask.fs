﻿module ProgramTask

open System.Threading.Tasks
open FSharp.SystemCommandLine
open Input

let app (ctx, words: string array, separator: string) =
    task {
        for i in [1..20] do
            if ctx.CancellationToken.IsCancellationRequested then 
                printfn "Cancellation Requested!"
                raise (new System.OperationCanceledException())
            else 
                printfn $"{i}"
                do! Task.Delay(1000)

        System.String.Join(separator, words)
        |> printfn "Result: %s"
    }
    
let main argv = 
    let words = option "--word" |> alias "-w" |> desc "A list of words to be appended"
    let separator = option "--separator" |> alias "-s" |> def ", " |> desc "A character that will separate the joined words."

    rootCommand argv {
        description "Appends words together"
        inputs (context, words, separator)
        setAction app
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

let run () = 
    "-w hello -w world" |> Utils.args |> main
