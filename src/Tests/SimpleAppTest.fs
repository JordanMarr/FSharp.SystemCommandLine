module SimpleAppTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils
open Input

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
            //Input.Option("--word", ["-w"], Array.empty, "A list of words to be appended"),
            //Input.OptionMaybe("--separator", ["-s"], "A character that will separate the joined words.")
            option "--word" |> alias "-w" |> desc "A list of words to be appended",
            optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."
        )
        setAction (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! Some "*"
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``02 --word Hello -w World return unit`` () =    
    testRootCommand "--word Hello -w World" {
        description "Appends words together"
        inputs (
            option "--word" |> alias "-w" |> desc "A list of words to be appended",
            optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."
        )
        setAction (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! None
            handlerCalled <- true
        )
    } =! 0

[<Test>]
let ``03 --word Hello -w World -s * return int`` () =    
    testRootCommand "--word Hello -w World -s *" {
        description "Appends words together"
        inputs (
            option "--word" |> alias "-w" |> desc "A list of words to be appended",
            optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."
        )
        setAction (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! Some "*"
            handlerCalled <- true
            5
        )
    } =! 5

[<Test>]
let ``04 --word Hello -w World -s * return int using manual configured options`` () =    
    testRootCommand "--word Hello -w World -s *" {
        description "Appends words together"
        inputs (
            option "--word" |> alias "-w" |> desc "A list of words to be appended",
            optionMaybe "--separator" |> alias "-s" |> desc "A character that will separate the joined words."
        )
        setAction (fun (words, separator) ->
            words =! [| "Hello"; "World" |]
            separator =! Some "*"
            handlerCalled <- true
            5
        )
    } =! 5

[<Test>]
let ``05 empty array`` () = 
    testRootCommand "-s *" {
        description "Appends words together"
        inputs (
            option "--word" |> alias "-w" |> desc "A list of words to be appended",
            optionMaybe "--separator" |> aliases ["-s"] |> desc "A character that will separate the joined words."
        )
        setAction (fun (words, separator) ->
            words =! [||]
            separator =! Some "*"
            handlerCalled <- true
        )
    } =! 0

/// In beta5, the action handler is never called if an input starts with "@", even if ResponseFileTokenReplacer is set to null.
[<Test; Ignore "Ignore failing test">]
let ``06 Token Replacer`` () = 
    testRootCommand "--package @shoelace-style/shoelace" {
        description "Can be called with a leading @ package"
        configure (fun cfg -> 
            // Skip @ processing
            //cfg.UseTokenReplacer(fun _ _ _ -> false) // Removed in beta5
            cfg.ResponseFileTokenReplacer <- null // in beta5, you must set ResponseFileTokenReplacer to null to skip @ processing
            //cfg.ResponseFileTokenReplacer <- new TryReplaceToken(fun _ _ _ -> false)
        )
        //inputs (Input.Option<string>("package", [ "--package"; "-p" ], "A package with a leading @ name"))
        inputs (option "--package" |> aliases ["-p"] |> desc "A package with a leading @ name")
        setAction (fun (package: string) ->
            handlerCalled <- true
            if package.StartsWith("@") then
                printfn $"{package}"
                0 // success
            else
                eprintfn "The package name does not start with a leading @"
                1 // failure
        )
    } =! 0
    
[<Test>]
let ``07 - Validators`` () = 
    handlerCalled <- true // Set to true to avoid false negatives in the test
    let args = args "-w delete -s *"
    let cfg = 
        commandLineConfiguration {
            description "Appends words together"
            inputs (
                option<string[]> "--word" 
                |> alias "-w" 
                |> desc "A list of words to be appended"
                |> required
                |> validate (fun words -> 
                    if words |> Array.contains "delete" 
                    then Error "Word 'delete' is not allowed." 
                    else Ok ()
                ),

                optionMaybe<string> "--separator" 
                |> alias "-s" 
                |> desc "A character that will separate the joined words."
            )

            setAction (fun (words, separator) ->
                () // Should not be called due to validation failure
            )
        }

    printfn $"{cfg.Error}"
    let result = cfg.Invoke(args)
    result =! 1 // Expecting a failure due to the separator validation