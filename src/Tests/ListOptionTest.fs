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
let ``01 - No input to string list option should be empty list``() =
    let input = option<string list> "-p"
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
    
[<Test>]
let ``02 - No input to int list option should be empty list``() =
    let input = option<int list> "-p"
    let commandRunner (shouldSucceed: bool): string -> (int list -> bool) -> unit = fun command comp ->
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
    shouldSucceed "-p 3" (List.isEmpty >> not)
    shouldFail "-p a" List.isEmpty


[<Test>]
let ``03 - No input to string list argument should be empty list``() =
    let input = argument<string list> "p"
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
    shouldSucceed "a" (List.isEmpty >> not)
    shouldFail "a" List.isEmpty
    
[<Test>]
let ``04 - No input to int list argument should be empty list``() =
    let input = argument<int list> "p"
    let commandRunner (shouldSucceed: bool): string -> (int list -> bool) -> unit = fun command comp ->
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
    shouldSucceed "3" (List.isEmpty >> not)
    shouldFail "a" List.isEmpty
