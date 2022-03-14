module ProgramTask

open FSharp.SystemCommandLine
open System.CommandLine.Builder
open System.Threading.Tasks

type WordService() = 
    member _.Join(separator: string, words: string array) = 
        task {
            do! Task.Delay(1000)
            return System.String.Join(separator, words)
        }

let app (svc: WordService) (words: string array, separator: string) =
    task {
        let! result = svc.Join(separator, words)
        result |> printfn "Result: %s"
    }
    
//[<EntryPoint>]
let main argv = 
    let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], (fun () -> ", "), "A character that will separate the joined words.")

    // Initialize app dependencies
    let svc = WordService()

    rootCommand {
        description "Appends words together"
        inputs (words, separator)
        usePipeline (fun builder -> 
            builder.UseTypoCorrections(3)   // Override pipeline
        )
        setHandler (app svc)                // Partially apply app dependencies
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
