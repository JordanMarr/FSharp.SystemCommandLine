module SimpleAsyncTest

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
let ``01 --word Hello -w World -s * return Task`` () = task {
    let! resultCode = 
        testRootCommand "--word Hello -w World -s *" {
            description "Appends words together"
            inputs (
                Input.Option<string[]>("word", ["--word"; "-w"], Array.empty, "A list of words to be appended"),
                Input.OptionMaybe("separator", ["--separator"; "-s"], "A character that will separate the joined words.")
            )
            setAction (fun (words, separator) -> 
                task {
                    words =! [| "Hello"; "World" |]
                    separator =! Some "*"
                    handlerCalled <- true
                }
            )
        }

    resultCode =! 0
}

[<Test>]
let ``02 --word Hello -w World return Task`` () = task {
    let! resultCode = 
        testRootCommand "--word Hello -w World" {
            description "Appends words together"
            inputs (
                Input.Option("word", ["--word"; "-w"], Array.empty, "A list of words to be appended"),
                Input.OptionMaybe("separator", ["--separator"; "-s"], "A character that will separate the joined words.")
            )
            setAction (fun (words, separator) ->
                task {
                    words =! [| "Hello"; "World" |]
                    separator =! None
                    handlerCalled <- true
                }
            )
        }

    resultCode =! 0
}

[<Test>]
let ``03 --word Hello -w World return Task<int>`` () = task {
    let! returnedCode =
        testRootCommand "--word Hello -w World" {
            description "Appends words together"
            inputs (
                Input.Context(),
                Input.Option("word", ["--word"; "-w"], Array.empty, "A list of words to be appended"),
                Input.OptionMaybe("separator", ["--separator"; "-s"], "A character that will separate the joined words.")
            )
            setAction (fun (ctx, words, separator) ->
                task {
                    ctx.CancellationToken.IsCancellationRequested =! false
                    words =! [| "Hello"; "World" |]
                    separator =! None
                    handlerCalled <- true
                    return 5
                }
            )
        }

    returnedCode =! 5
}



