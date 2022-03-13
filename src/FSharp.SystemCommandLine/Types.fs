[<AutoOpen>]
module FSharp.SystemCommandLine.Types

open System
open System.CommandLine

/// A helper static class that creates `Option` and `Argument` `IValueDescriptor<'T>` objects.
type Input<'T> = 

    static member Option<'T>(name: string, getDefaultValue: (unit -> 'T), ?description: string) =
        Option<'T>(
            name,
            getDefaultValue = Func<'T>(getDefaultValue),
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(name: string, ?description: string) =
        Option<'T>(
            name,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(aliases: string[], getDefaultValue: (unit -> 'T), ?description: string) =
        Option<'T>(
            aliases,
            getDefaultValue = Func<'T>(getDefaultValue),
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(aliases: string[], ?description: string) =
        Option<'T>(
            aliases,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    // TODO: Add Option overloads 5 & 6 with ParseArgument<'T>
