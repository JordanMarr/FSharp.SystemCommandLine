module ArgumentMaybeTest

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
let ``01 Some jdoe`` () =    
    testRootCommand "jdoe" {
        description "Maybe displays a name"
        inputs (Input.ArgumentMaybe<string>("Maybe a name"))
        setAction (fun name ->
            name =! Some "jdoe"
            handlerCalled <- true
        )
    } =! 0
    
[<Test>]
let ``02 None`` () =    
    testRootCommand "" {
        description "Maybe displays a name"
        inputs (Input.ArgumentMaybe<string>("Maybe a name"))
        setAction (fun name ->
            name =! None
            handlerCalled <- true
        )
    } =! 0
