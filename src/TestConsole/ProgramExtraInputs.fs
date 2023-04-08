/// This option can be used when more than 8 inputs are required.
module ProgramExtraInputs

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

let app (ctx: System.CommandLine.Invocation.InvocationContext) =
    [ 
        a.GetValue ctx
        b.GetValue ctx
        c.GetValue ctx
        d.GetValue ctx
        e.GetValue ctx
        f.GetValue ctx |> string
        g.GetValue ctx |> string
        h.GetValue ctx |> string
        i.GetValue ctx |> string
        j.GetValue ctx |> string
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
        setHandler app
        addInputs [ a; b; c; d; e; f; g; h; i; j ]
    }
