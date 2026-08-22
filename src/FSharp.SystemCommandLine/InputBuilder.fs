[<AutoOpen>]
module FSharp.SystemCommandLine.InputBuilder

open System.CommandLine
open System.Threading

/// Composes multiple inputs into a single `ActionInput<'T>` using applicative `let!`/`and!` bindings.
/// `Bind` is deliberately absent: a sequential `let!` would allow one input to depend on another's
/// parsed value, which cannot be known before parsing, so the composed input set would not be
/// statically registerable. Bind every source in one `let!`/`and!` group.
type InputBuilder() =

    member _.MergeSources(a: ActionInput<'A>, b: ActionInput<'B>) : ActionInput<'A * 'B> =
        Composed (
            a.Flatten() @ b.Flatten(),
            fun parseResult cancelToken -> box (a.GetValue(parseResult, cancelToken), b.GetValue(parseResult, cancelToken))
        )
        |> ActionInput<'A * 'B>

    member _.BindReturn(input: ActionInput<'A>, mapping: 'A -> 'B) : ActionInput<'B> =
        Composed (
            input.Flatten(),
            fun parseResult cancelToken -> input.GetValue(parseResult, cancelToken) |> mapping |> box
        )
        |> ActionInput<'B>

    member _.Return(value: 'T) : ActionInput<'T> =
        Composed ([], fun _ _ -> box value) |> ActionInput<'T>

    member _.ReturnFrom(input: ActionInput<'T>) : ActionInput<'T> =
        input

/// Composes multiple inputs into a single `ActionInput<'T>` using applicative `let!`/`and!` bindings.
/// The composed input can be passed to the `inputs` operation, included in an `inputs` tuple,
/// yielded directly inside a `rootCommand` or `command` expression, or nested within another `input { }`.
let input = InputBuilder()
