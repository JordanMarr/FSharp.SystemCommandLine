module ProgramAppendWords

open FSharp.SystemCommandLine
open Input

let app (words: string array, separator: string option) =
    let separator = defaultArg separator " + "
    System.String.Join(separator, words) |> printfn "Result: %s"
    
let main argv = 
    rootCommand argv {
        description "Appends words together"
        inputs (
            option "--word" 
            |> alias "-w" 
            |> desc "A list of words to be appended" 
            |> arity OneOrMore
            |> acceptOnlyFromAmong [ "hello" ; "world" ], 

            optionMaybe "--separator"
            |> alias "-s"
            |> desc "A character that will separate the joined words."
        )
        setAction app
    }

let run () = 
    "-w hello -w world -s \", \"" |> Utils.args |> main
    