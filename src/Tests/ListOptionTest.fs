module ListOptionTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils
open Input

/// Runs `command` against a root command with the single list `input`.
/// Returns `Some values` if the action ran, or `None` if parsing failed.
let run (input: ActionInput<'T list>) (command: string) =
    let mutable received = None
    let exitCode =
        testRootCommand command {
            description "Test"
            inputs input
            setAction (fun values -> received <- Some values; 0)
        }
    match received with
    | Some _ -> exitCode =! 0
    | None -> exitCode <>! 0
    received

[<Test>]
let ``01 - string list option`` () =
    let input = option<string list> "-p"
    run input "" =! Some []
    run input "-p a" =! Some [ "a" ]
    run input "-p a -p b -p c" =! Some [ "a"; "b"; "c" ]

[<Test>]
let ``02 - int list option`` () =
    let input = option<int list> "-p"
    run input "" =! Some []
    run input "-p 3 -p 1 -p 2" =! Some [ 3; 1; 2 ]
    run input "-p a" =! None
    run input "-p 99999999999" =! None

[<Test>]
let ``03 - string list argument`` () =
    let input = argument<string list> "p"
    run input "" =! Some []
    run input "a b" =! Some [ "a"; "b" ]

[<Test>]
let ``04 - int list argument`` () =
    let input = argument<int list> "p"
    run input "" =! Some []
    run input "3 4" =! Some [ 3; 4 ]
    run input "a" =! None

[<Test>]
let ``05 - FileInfo list option`` () =
    let input = option<System.IO.FileInfo list> "-f"
    let names = run input "-f a.txt -f b.txt" |> Option.map (List.map _.Name)
    names =! Some [ "a.txt"; "b.txt" ]

[<Test>]
let ``06 - Guid list option reports unsupported type as a parse error`` () =
    let input = option<System.Guid list> "-g"
    run input "-g 7f3c2a10-0000-0000-0000-000000000000" =! None
