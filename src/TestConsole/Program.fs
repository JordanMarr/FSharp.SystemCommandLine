module Program

open FSharp.SystemCommandLine
open Input

let app (words: string array, separator: string option) =
    let separator = separator |> Option.defaultValue ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
[<EntryPoint>]
let main argv = 
    let words = option "--word" |> alias "-w" |> def [||] |> desc "A list of words to be appended"
    let separator = optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."

    rootCommand argv {
        description "Appends words together"
        inputs (words, separator)
        setAction app
    }