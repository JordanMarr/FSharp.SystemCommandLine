module OptionMaybeStringTest

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
let ``01 --str with argument value should be Some value`` () =
    testRootCommand "--str value" {
        description "Test"
        inputs (Input.OptionMaybe<string>("--str", "Just a string"))
        setAction (fun str ->
            str =! Some "value"
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``02 no option or argument should be None`` () =
    testRootCommand "" {
        description "Test"
        inputs (Input.OptionMaybe<string>("--str", "Just a string"))
        setAction (fun str ->
            str =! None
            handlerCalled <- true
        )
    } =! 0
