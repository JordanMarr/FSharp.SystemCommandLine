module CmdTests

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils

let cmd (handler: string[] * string -> unit) (args: string) =
    let oWords = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let oSeparator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")
    
    testRootCommand (splitBySpace args) {
        description "Appends words together"
        inputs (oWords, oSeparator)
        setHandler handler
    } 
    |> ignore

[<Test>]
let Test1 () =
    let handler (words: string array, separator: string) =
        System.String.Join(separator, words)
        |> printfn "Result: %s"

    cmd handler "--word Hello -w World -s ,"
        
