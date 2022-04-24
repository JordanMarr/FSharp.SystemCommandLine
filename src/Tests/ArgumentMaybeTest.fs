module ArgumentMaybeTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

let nameMaybe = Input.ArgumentMaybe<string>("Maybe a name")

let rootCmd argstr (handler: string option -> unit) = 
    testRootCommand argstr {
        description "Maybe displays a name"
        inputs (nameMaybe)
        setHandler handler
    } 
    |> ignore

[<Test>]
let ``01 Some jdoe`` () =    
    let mutable handlerCalled = false
    rootCmd "jdoe"
        (fun (nameMaybe) ->
            handlerCalled <- true
            nameMaybe =! Some "jdoe"
        )

    handlerCalled =! true
    
[<Test>]
let ``02 None`` () =    
    let mutable handlerCalled = false
    rootCmd ""
        (fun (nameMaybe) ->
            handlerCalled <- true
            nameMaybe =! None
        )

    handlerCalled =! true