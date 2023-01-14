/// This option can be used when more than 8 inputs are required.
module ProgramExtraInputs

open FSharp.SystemCommandLine

module Parameters = 
    let words = Input.Option<string[]>(["--word"; "-w"], Array.empty, "A list of words to be appended")
    let separator = Input.OptionMaybe<string>(["--separator"; "-s"], "A character that will separate the joined words.")

let app (ctx: System.CommandLine.Invocation.InvocationContext) =
    // Manually parse parameters
    let words = Parameters.words.GetValue ctx
    let separator = Parameters.separator.GetValue ctx

    // Append words together
    let separator = separator |> Option.defaultValue ", "
    System.String.Join(separator, words) |> printfn "Result: %s"
    0
    
//[<EntryPoint>]
let main argv = 
    let ctx = Input.Context()

    rootCommand argv {
        description "Appends words together"
        inputs ctx
        setHandler app
        add Parameters.words
        add Parameters.separator
    }
