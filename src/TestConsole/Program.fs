module Program

open FSharp.SystemCommandLine

let app (words: string array, separator: string option) =
    let separator = separator |> Option.defaultValue ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
[<EntryPoint>]
let main argv = 
    let words = Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended")
    let separator = Input.OptionMaybe(["--separator"; "-s"], "A character that will separate the joined words.")

    rootCommand argv {
        description "Appends words together"
        inputs (words, separator)
        setHandler app
    }