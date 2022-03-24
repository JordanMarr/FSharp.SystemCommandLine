module SimpleAppTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

let words = Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended")
let separator = Input.Option(["--separator"; "-s"], ",", "A character that will separate the joined words.")

let rootCmd argstr (handler: string array * string -> unit) = 
    testRootCommand argstr {
        description "Appends words together"
        inputs (words, separator)
        setHandler handler
    } 
    |> ignore

[<Test>]
let ``01 --word Hello -w World -s *`` () =    
    let mutable handlerCalled = false
    rootCmd "--word Hello -w World -s *"
        (fun (words, separator) ->
            handlerCalled <- true
            words =! [| "Hello"; "World" |]
            separator =! "*"
        )

    handlerCalled =! true
    