open FSharp.SystemCommandLine

let app (words: string array, separator: string) =
    System.String.Join(separator, words)
    |> printfn "Result: %s"
    
[<EntryPoint>]
let main argv = 
    let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")

    rootCommand {
        description "Appends words together"
        inputs (words, separator)
        setHandler app
    }
    