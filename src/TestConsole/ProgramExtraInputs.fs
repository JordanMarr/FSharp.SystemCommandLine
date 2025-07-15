/// This option can be used when more than 8 inputs are required.
module ProgramExtraInputs

open FSharp.SystemCommandLine
open Input

let a = option "-a" |> defaultValue "a?"
let b = option "-b" |> defaultValue "b?"
let c = option "-c" |> defaultValue "c?"
let d = option "-d" |> defaultValue "d?"
let e = option "-e" |> defaultValue "e?"
let f = option "-1" |> defaultValue 0
let g = option "-2" |> defaultValue 0
let h = option "-3" |> defaultValue 0 // NOTE: "-h" is taken via Help pipeline defaults
let i = option "-4" |> defaultValue 0
let j = option "-5" |> defaultValue 0

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
    
let main argv = 
    rootCommand argv {
        description "Appends words together"
        inputs Input.context
        setAction app
        addInputs [ a; b; c; d; e; f; g; h; i; j ]
    }

let run () = 
    "-a A -b B -c C -d D -e E -1 1 -2 2 -3 3 -4 4 -5 5" |> Utils.args |> main