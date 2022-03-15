module SimpleAppTest

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils
open FsUnit

[<Test>]
let Test1 () =    
    let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")

    testRootCommand "--word Hello -w World -s *" {
        description "Appends words together"
        inputs (words, separator)
        setHandler (
            fun (words: string array, separator: string) ->
                words |> should equal [| "Hello"; "World" |]
                separator |> should equal "*"
        )
    } 
    |> ignore
        
