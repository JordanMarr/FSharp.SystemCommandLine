module SimpleAppTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

let mutable handlerCalled = false
[<SetUp>] 
let setup () = handlerCalled <- false
[<TearDown>] 
let tearDown () = handlerCalled =! true

[<Test>]
let ``01 --word Hello -w World -s * return unit`` () =    
    testRootCommand "--word Hello -w World -s *" {
        description "Appends words together"
        inputs (
            Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended"), 
            Input.OptionMaybe(["--separator"; "-s"], "A character that will separate the joined words.")
        )
        setHandler (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! Some "*"
            handlerCalled <- true
        )
    } |> ignore

[<Test>]
let ``02 --word Hello -w World return unit`` () =    
    testRootCommand "--word Hello -w World" {
        description "Appends words together"
        inputs (
            Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended"), 
            Input.OptionMaybe(["--separator"; "-s"], "A character that will separate the joined words.")
        )
        setHandler (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! None
            handlerCalled <- true
        )
    } |> ignore

[<Test>]
let ``03 --word Hello -w World -s * return int`` () =    
    let code = 
        testRootCommand "--word Hello -w World -s *" {
            description "Appends words together"
            inputs (
                Input.Option(["--word"; "-w"], Array.empty, "A list of words to be appended"), 
                Input.OptionMaybe(["--separator"; "-s"], "A character that will separate the joined words.")
            )
            setHandler (fun (words, separator) ->
                words =! [| "Hello"; "World" |]
                separator =! Some "*"
                handlerCalled <- true
                5
            )
        }

    code =! 5
