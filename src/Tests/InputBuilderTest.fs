module InputBuilderTest

open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

let mutable actionCalled = false
[<SetUp>]
let setup () = actionCalled <- false

[<Test>]
let ``01 - Inline input block yielded directly in the command CE`` () =
    testRootCommand "--word Hello -w World -s *" {
        description "Appends words together"
        input {
            let! words = Input.option<string[]> "--word" |> Input.alias "-w" |> Input.desc "A list of words to be appended"
            and! separator = Input.optionMaybe<string> "--separator" |> Input.alias "-s" |> Input.desc "A character that will separate the joined words."
            return {| Words = words; Separator = separator |}
        }
        setAction (fun io ->
            io.Words =! [| "Hello"; "World" |]
            io.Separator =! Some "*"
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``02 - Composed input defined outside and passed to the inputs operation`` () =
    let io = input {
        let! words = Input.option<string[]> "--word" |> Input.alias "-w"
        and! separator = Input.option<string> "--separator" |> Input.alias "-s" |> Input.def ", "
        return {| Words = words; Separator = separator |}
    }

    testRootCommand "--word Hello -w World" {
        description "Appends words together"
        inputs io
        setAction (fun io ->
            io.Words =! [| "Hello"; "World" |]
            io.Separator =! ", "
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``03 - Composed input mixed into an inputs tuple with a plain input`` () =
    let io = input {
        let! words = Input.option<string[]> "--word" |> Input.alias "-w"
        and! separator = Input.option<string> "--separator" |> Input.alias "-s" |> Input.def ", "
        return {| Words = words; Separator = separator |}
    }

    testRootCommand "--word Hello --verbose true" {
        description "Appends words together"
        inputs (io, Input.option<bool> "--verbose")
        setAction (fun (io, verbose) ->
            io.Words =! [| "Hello" |]
            io.Separator =! ", "
            verbose =! true
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``04 - Nested composed inputs`` () =
    let common = input {
        let! verbose = Input.option<bool> "--verbose"
        and! config = Input.option<string> "--configuration" |> Input.def "Release"
        return {| Verbose = verbose; Config = config |}
    }

    testRootCommand "--verbose true --quick" {
        description "Builds the solution"
        input {
            let! common = common
            and! quick = Input.option<bool> "--quick"
            return {| Common = common; Quick = quick |}
        }
        setAction (fun io ->
            io.Common.Verbose =! true
            io.Common.Config =! "Release"
            io.Quick =! true
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``05 - More than 8 inputs`` () =
    testRootCommand "--i1 1 --i2 2 --i3 3 --i4 4 --i5 5 --i6 6 --i7 7 --i8 8 --i9 9 --i10 10" {
        description "Beyond the tuple limit"
        input {
            let! i1 = Input.option<int> "--i1"
            and! i2 = Input.option<int> "--i2"
            and! i3 = Input.option<int> "--i3"
            and! i4 = Input.option<int> "--i4"
            and! i5 = Input.option<int> "--i5"
            and! i6 = Input.option<int> "--i6"
            and! i7 = Input.option<int> "--i7"
            and! i8 = Input.option<int> "--i8"
            and! i9 = Input.option<int> "--i9"
            and! i10 = Input.option<int> "--i10"
            return [ i1; i2; i3; i4; i5; i6; i7; i8; i9; i10 ]
        }
        setAction (fun values ->
            values =! [ 1; 2; 3; 4; 5; 6; 7; 8; 9; 10 ]
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``06 - Shared input bound by two composed specs registers only once`` () =
    let sharedConfig = Input.option<string> "--configuration" |> Input.def "Release"

    let specA = input {
        let! config = sharedConfig
        and! quick = Input.option<bool> "--quick"
        return {| Config = config; Quick = quick |}
    }

    let specB = input {
        let! config = sharedConfig
        and! verbose = Input.option<bool> "--verbose"
        return {| Config = config; Verbose = verbose |}
    }

    testRootCommand "--configuration Debug --quick --verbose true" {
        description "Shared options are deduped"
        inputs (specA, specB)
        setAction (fun (a, b) ->
            a.Config =! "Debug"
            b.Config =! "Debug"
            a.Quick =! true
            b.Verbose =! true
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``07 - Inline input block returning int`` () =
    testRootCommand "--value 21" {
        description "Returns an int status code"
        input {
            let! value = Input.option<int> "--value"
            return value
        }
        setAction (fun value ->
            actionCalled <- true
            value * 2
        )
    } =! 42
    actionCalled =! true

[<Test>]
let ``08 - Input context inside the input block`` () =
    testRootCommand "--name Jordan" {
        description "Context flows through composed inputs"
        input {
            let! name = Input.option<string> "--name"
            and! ctx = Input.context
            return {| Name = name; Ctx = ctx |}
        }
        setAction (fun io ->
            io.Name =! "Jordan"
            io.Ctx.ParseResult.GetValue<string>("--name") =! "Jordan"
            actionCalled <- true
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``09 - configureParser before an inline input block is preserved`` () =
    testRootCommand "--package @shoelace-style/shoelace" {
        description "Can be called with a leading @ package"
        configureParser (fun cfg -> cfg.ResponseFileTokenReplacer <- null)
        input {
            let! package = Input.option<string> "--package"
            return package
        }
        setAction (fun (package: string) ->
            actionCalled <- true
            package =! "@shoelace-style/shoelace"
        )
    } =! 0
    actionCalled =! true

[<Test>]
let ``10 - Inline input block in a subcommand`` () =
    let getCmd =
        command "get" {
            description "Get a package by name"
            input {
                let! package = Input.argument<string> "package"
                and! verbose = Input.option<bool> "--verbose"
                return {| Package = package; Verbose = verbose |}
            }
            setAction (fun io ->
                io.Package =! "FSharp.SystemCommandLine"
                io.Verbose =! true
                actionCalled <- true
            )
        }

    testRootCommand "get FSharp.SystemCommandLine --verbose true" {
        description "Package manager"
        noAction
        addCommand getCmd
    } =! 0
    actionCalled =! true
