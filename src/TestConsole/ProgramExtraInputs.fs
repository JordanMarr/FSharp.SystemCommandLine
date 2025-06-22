/// This option can be used when more than 8 inputs are required.
module ProgramExtraInputs

open FSharp.SystemCommandLine
open Input

let a = option "-a" |> def "a?"
let b = option "-b" |> def "b?"
let c = option "-c" |> def "c?"
let d = option "-d" |> def "d?"
let e = option "-e" |> def "e?"
let f = option "-1" |> def 0
let g = option "-2" |> def 0
let h = option "-3" |> def 0 // NOTE: "-h" is taken via Help pipeline defaults
let i = option "-4" |> def 0
let j = option "-5" |> def 0

let app ctx =
    [
        a.GetValue ctx.ParseResult
        b.GetValue ctx.ParseResult
        c.GetValue ctx.ParseResult
        d.GetValue ctx.ParseResult
        e.GetValue ctx.ParseResult
        f.GetValue ctx.ParseResult |> string
        g.GetValue ctx.ParseResult |> string
        h.GetValue ctx.ParseResult |> string
        i.GetValue ctx.ParseResult |> string
        j.GetValue ctx.ParseResult |> string
    ]
    |> String.concat ", "
    |> printfn "Result: %s"
    0
    
//[<EntryPoint>]
let main argv = 
    let ctx = Input.Context()

    rootCommand argv {
        description "Appends words together"
        inputs ctx
        setAction app
        addInputs [ a; b; c; d; e; f; g; h; i; j ]
    }
