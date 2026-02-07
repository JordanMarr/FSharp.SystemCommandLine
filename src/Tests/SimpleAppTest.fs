module SimpleAppTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils
open Input
open System.CommandLine.Parsing

let mutable actionCalled = false
[<SetUp>] 
let setup () = actionCalled <- false

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
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

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
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

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
            actionCalled <- true
            5
        )
    } =! 5
    actionCalled =! true

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
            actionCalled <- true
            5
        )
    } =! 5
    actionCalled =! true

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
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

/// In beta5, the action handler is never called if an input starts with "@", even if ResponseFileTokenReplacer is set to null.
[<Test>]
let ``06 - rootCommand should use configuration`` () = 
    testRootCommand "--package @shoelace-style/shoelace" {
        description "Can be called with a leading @ package"
        configureParser (fun cfg -> 
            // Skip @ processing
            cfg.ResponseFileTokenReplacer <- null
        )
        //inputs (Input.Option<string>("package", [ "--package"; "-p" ], "A package with a leading @ name"))
        inputs (option "--package" |> aliases ["-p"] |> desc "A package with a leading @ name")
        setAction (fun (package: string) ->
            actionCalled <- true
            if package.StartsWith("@") then
                printfn $"{package}"
                0 // success
            else
                eprintfn "The package name does not start with a leading @"
                1 // failure
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``07 - Child command should use configuration`` () = 
    let getCmd = 
        command "get" {
            description "Get a package by name"
            inputs (
                argument<string> "package" 
                |> desc "A package with a leading @ name"
            )
            setAction (fun (package: string) ->
                actionCalled <- true
                if package.StartsWith("@") then
                    printfn $"{package}"
                    0 // success
                else
                    eprintfn "The package name does not start with a leading @"
                    1 // failure
            )
        }

    testRootCommand "get @shoelace-style/shoelace" {
        description "Can be called with a leading @ package"
        configureParser (fun cfg -> 
            // Skip @ processing
            cfg.ResponseFileTokenReplacer <- null
        )        
        noAction
        addCommand getCmd
    } =! 0
    actionCalled =! true

[<Test>]
let ``08 - Validators`` () = 
    let args = args "-w delete -s *"
    let rootCmd = 
        ManualInvocation.rootCommand {
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

    let parseResult = rootCmd.Parse(args)
    printfn $"{parseResult.Errors}"
    
    let result = parseResult.Invoke()
    result =! 1 // Expecting a failure due to the separator validation
    actionCalled =! false

[<Test>]
let ``09 tryParser Directory Info`` () = 
    testRootCommand "--directory \"c:/fake\"" {
        description "Custom parser for directory info"
        inputs (
            option "--directory" 
            |> desc "A directory path"
            |> required
            |> tryParse (fun res ->
                let path = res.Tokens[0].Value
                if System.IO.Directory.Exists path
                then Ok (System.IO.DirectoryInfo path)
                else Error $"'{path}' is not a valid directory."
            )
        )
        setAction (fun dir ->
            printfn $"Directory: {dir}"
        )
    } =! 1 // Should fail
    actionCalled =! false

type AppSettings = { ConnectionString: string; MaxRetries: int }

[<Test>]
let ``10 - Input_inject should pass a dependency into the action`` () =
    let settings = { ConnectionString = "Server=localhost"; MaxRetries = 3 }

    testRootCommand "--name Jordan" {
        description "Inject a dependency"
        inputs (
            option<string> "--name" |> desc "Your name",
            inject settings
        )
        setAction (fun (name, injectedSettings) ->
            name =! "Jordan"
            injectedSettings =! settings
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

type Env = { GetRandom: int -> int -> int }

[<Test>]
let ``11 - Input_inject should allow injecting behavior into a pure action`` () =
    let env = { GetRandom = fun min max -> 42 }

    testRootCommand "--count 5" {
        description "Generate random numbers"
        inputs (
            inject env,
            option<int> "--count" |> desc "How many numbers to generate"
        )
        setAction (fun (env, count) ->
            let result = env.GetRandom 1 100
            count =! 5
            result =! 42
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true
