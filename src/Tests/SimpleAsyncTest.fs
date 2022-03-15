module SimpleAsyncTest

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils
open FsUnit
open System.Threading.Tasks

let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
let separator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")

let rootCmd argstr (handler: string array * string -> unit) = 
    task {
        testRootCommand argstr {
            description "Appends words together"
            inputs (words, separator)
            setHandler handler
        } 
        |> ignore
    }

[<Test>]
let ``01 --word Hello -w World -s *`` () =
    rootCmd "--word Hello -w World -s *"
        (fun (words, separator) ->
            words |> should equal [| "Hello"; "World" |]
            separator |> should equal "*"
        )
    