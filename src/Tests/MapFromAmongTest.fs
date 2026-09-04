module MapFromAmongTest


open System
open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils
open Input

let mutable actionCalled = false
let callAction() = actionCalled <- true
[<SetUp>]
let setup () = actionCalled <- false

type DUType =
    | A
    | B

let duChoices = [
    "a", A
    B.ToString() (* "B" *), B
]

[<Test>]
let ``01 - mapFromAmong requires input``() =
    let input =
        option<DUType> "--du" |> mapFromAmong duChoices
    testRootCommand "--du" {
        description "Test"
        inputs input
        setAction (ignore >> callAction)
    } <>! 0
    actionCalled <>! true
    
[<Test>]
let ``02 - mapFromAmong returns correct typed DU value``() =
    let input = option<DUType> "--du" |> mapFromAmong duChoices
    let compareAgainst (shouldSucceed: bool): string -> DUType -> unit = fun cmd v ->
        testRootCommand cmd {
            description "Test"
            inputs input
            setAction (fun o ->
                if shouldSucceed
                then o =! v |> callAction; 0
                else o <>! v; 1
                )
        }
        |> if shouldSucceed then (=!) 0 else (<>!) 0
        actionCalled =! shouldSucceed
        actionCalled <- false
    let shouldSucceed = compareAgainst true
    let shouldFail = compareAgainst false
    // valid casing
    shouldSucceed "--du a" A
    shouldSucceed "--du B" B
    // invalid casing
    shouldFail "--du A" A
    shouldFail "--du b" B
    // invalid input
    shouldFail "--du c" A
    shouldFail "--du c" B
    // invalid map
    shouldFail "--du a" B
    shouldFail "--du B" A


[<Test>]
let ``03 - mapFromAmongWith returns correct typed DU - case insensitive``() =
    let input = option<DUType> "--du" |> mapFromAmongWith StringComparer.OrdinalIgnoreCase duChoices
    let compareAgainst (shouldSucceed: bool): string -> DUType -> unit = fun cmd v ->
        testRootCommand cmd {
            description "Test"
            inputs input
            setAction (fun o ->
                if shouldSucceed
                then o =! v |> callAction; 0
                else o <>! v; 1
                )
        }
        |> if shouldSucceed then (=!) 0 else (<>!) 0
        actionCalled =! shouldSucceed
        actionCalled <- false
    let shouldSucceed = compareAgainst true
    let shouldFail = compareAgainst false
    shouldSucceed "--du a" A
    shouldSucceed "--du A" A
    shouldSucceed "--du b" B
    shouldSucceed "--du B" B
    // invalid input
    shouldFail "--du c" A
    shouldFail "--du c" B
    // invalid map
    shouldFail "--du a" B
    shouldFail "--du B" A

[<Test>]
let ``04 - mapFromAmong followed by different tryParse will override``() =
    let input = option<DUType> "--du" |> mapFromAmong duChoices |> tryParse (fun _ -> Ok A)
    let compareAgainst (shouldSucceed: bool): string -> DUType -> unit = fun cmd v ->
        testRootCommand cmd {
            description "Test"
            inputs input
            setAction (fun o ->
                if shouldSucceed
                then o =! v |> callAction; 0
                else o <>! v; 1
                )
        }
        |> if shouldSucceed then (=!) 0 else (<>!) 0
        actionCalled =! shouldSucceed
        actionCalled <- false
    let shouldSucceed = compareAgainst true
    let shouldFail = compareAgainst false
    shouldSucceed "--du a" A
    shouldSucceed "--du A" A
    shouldSucceed "--du b" A
    shouldSucceed "--du B" A
    shouldFail "--du a" B
    shouldFail "--du A" B
    shouldFail "--du b" B
    shouldFail "--du B" B
    // invalid input still processes
    // completions will still show correctly
    // undefined behaviour
    shouldSucceed "--du c" A
    shouldFail "--du c" B
