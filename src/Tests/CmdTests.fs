module CmdTests

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils

let cmd (handler: string[] * string -> unit) (args: string) =
    let oWords = Input.Option(["--words"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let oSeparator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")

    let result = 
        testRootCommand (splitBySpace args) {
            description "Appends words together"
            inputs (oWords, oSeparator)
            setHandler handler
        }
    ()


[<Test>]
let Test1 () =
    let handler (words: string array, separator: string) =
        System.String.Join(separator, words)
        |> printfn "Result: %s"

    cmd handler "-w Hello -w World -s *"
        
