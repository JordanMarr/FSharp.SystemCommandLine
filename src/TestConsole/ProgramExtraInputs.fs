/// This option can be used when more than 8 inputs are required.
module ProgramExtraInputs

open System.CommandLine
open FSharp.SystemCommandLine

let a = Input.Option<string>("-a", defaultValue = "a?")
let b = Input.Option<string>("-b", defaultValue = "b?")
let c = Input.Option<string>("-c", defaultValue = "c?")
let d = Input.Option<string>("-d", defaultValue = "d?")
let e = Input.Option<string>("-e", defaultValue = "e?")
let f = Input.Option<int>("-1", defaultValue = 0)
let g = Input.Option<int>("-2", defaultValue = 0)
let h = Input.Option<int>("-3", defaultValue = 0) // NOTE: "-h" is taken via Help pipeline defaults
let i = Input.Option<int>("-4", defaultValue = 0)
let j = Input.Option<int>("-5", defaultValue = 0)

let app (parseResult: ParseResult) =
    [ 
        a.GetValue parseResult
        b.GetValue parseResult
        c.GetValue parseResult
        d.GetValue parseResult
        e.GetValue parseResult
        f.GetValue parseResult |> string
        g.GetValue parseResult |> string
        h.GetValue parseResult |> string
        i.GetValue parseResult |> string
        j.GetValue parseResult |> string
    ]
    |> String.concat ", "
    |> printfn "Result: %s"
    0
    
//[<EntryPoint>]
let main argv = 
    let parseResult = Input.ParseResult()

    rootCommand argv {
        description "Appends words together"
        inputs parseResult
        setHandler app
        addInputs [ a; b; c; d; e; f; g; h; i; j ]
    }
