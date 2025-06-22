module OptionMaybeBoolTest

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
let ``01 --flag with no argument should be true`` () =
    testRootCommand "--flag" {
        description "Test"
        inputs (Input.OptionMaybe<bool>("--flag", "True, false or none"))
        setAction (fun flag ->
            flag =! Some true
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``02 --flag with true argument should be Some true`` () =
    testRootCommand "--flag true" {
        description "Test"
        inputs (Input.OptionMaybe<bool>("--flag", "True, false or none"))
        setAction (fun flag ->
            flag =! Some true
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``03 --flag with false argument should be Some false`` () =
    testRootCommand "--flag false" {
        description "Test"
        inputs (Input.OptionMaybe<bool>("--flag", "True, false or none"))
        setAction (fun flag ->
            flag =! Some false
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``04 no option or argument should be None`` () =
    testRootCommand "" {
        description "Test"
        inputs (Input.OptionMaybe<bool>("--flag", "True, false or none"))
        setAction (fun flag ->
            flag =! None
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``05 no option or argument should be None with int return`` () =
    testRootCommand "--flag" {
        description "Test"
        inputs (Input.OptionMaybe<bool>("--flag", "True, false or none"))
        setAction (fun flag ->
            flag =! Some true
            handlerCalled <- true
            5
        )
    } =! 5

