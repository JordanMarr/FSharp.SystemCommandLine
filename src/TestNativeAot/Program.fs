open System
open System.Runtime.CompilerServices
open FSharp.SystemCommandLine
open Input

[<EntryPoint>]
let main argv =
    if RuntimeFeature.IsDynamicCodeSupported then
        failwith "This smoke test must run as a NativeAOT executable."

    rootCommand argv {
        description "NativeAOT startup and scalar/array parsing regression test"
        inputs (argument<string array> "words" |> arity ZeroOrMore, option<int> "--count")
        setAction (fun (words, count) ->
            // Avoid printf's own reflection requirements so this tests the library's input path.
            Console.WriteLine("words=" + String.Join(",", words) + ";count=" + count.ToString())
        )
    }
