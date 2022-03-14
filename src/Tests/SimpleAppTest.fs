module SimpleAppTest

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils
open FsUnit

let cmd (argString: string) (handler: string[] * string -> unit) =
    let words = Input.Option(["--word"; "-w"], (fun () -> Array.empty<string>), "A list of words to be appended")
    let separator = Input.Option(["--separator"; "-s"], (fun () -> ","), "A character that will separate the joined words.")
    
    testRootCommand argString {
        description "Appends words together"
        inputs (words, separator)
        setHandler handler
    } 
    |> ignore

[<Test>]
let Test1 () =
    let handler (words: string array, separator: string) =
        words |> should equal [| "Hello"; "World" |]
        separator |> should equal "*"
        System.String.Join(separator, words) |> printfn "Result: %s"

    cmd "--word Hello -w World -s *" handler
        
