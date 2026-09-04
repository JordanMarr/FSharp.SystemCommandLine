module ListOptionTest


open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils
open Input

let mutable handlerCalled = false
let called() = handlerCalled <- true
[<SetUp>] 
let setup () = handlerCalled <- false

[<Test>]
let ``01 - No input to list option should be empty list``() =
    let input = option<string list> "-p" |> arity Arity.ZeroOrMore
    let commandRunner (shouldSucceed: bool): string -> (string list -> bool) -> unit = fun command comp ->
        testRootCommand command {
            description "Test"
            inputs input
            setAction (function
                | values when comp values -> called(); 0
                | _ -> 1
                )
        }
        |> if shouldSucceed then (=!) 0 else (<>!) 0
        handlerCalled =! shouldSucceed
        handlerCalled <- false
    let shouldSucceed = commandRunner true
    let shouldFail = commandRunner false
    
    shouldSucceed "" List.isEmpty
    shouldSucceed "-p a" (List.isEmpty >> not)
    shouldFail "-p a" List.isEmpty
