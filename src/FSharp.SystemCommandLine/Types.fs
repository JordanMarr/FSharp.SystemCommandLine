[<AutoOpen>]
module FSharp.SystemCommandLine.Types

open System
open System.CommandLine

module private Parser = 
    /// Parses an argument token value. 
    /// TODO: Ideally, this should use the S.CL Arugment parser.
    let parseArgumentTokenValue<'T when 'T : null> (tokenValue: string) = 
        match typeof<'T> with
        | t when t = typeof<IO.DirectoryInfo> -> IO.DirectoryInfo(tokenValue) |> box :?> 'T |> Some
        | t when t = typeof<IO.FileInfo> -> IO.FileInfo(tokenValue) |> box :?> 'T |> Some
        | t -> Convert.ChangeType(tokenValue, t) :?> 'T |> Option.ofObj

/// A helper static class that creates `Option` and `Argument` `IValueDescriptor<'T>` objects.
type Input = 

    static member OptionMaybe<'T when 'T : null>(name: string, ?description: string) =
        System.CommandLine.Option<'T option>(
            name,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.tryHead with
                | Some token -> Parser.parseArgumentTokenValue token.Value
                | None -> failwith "F# Option can only be used with a single argument."
            ),            
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T option>

    static member OptionMaybe<'T when 'T : null>(aliases: string seq, ?description: string) =
        System.CommandLine.Option<'T option>(
            aliases |> Seq.toArray,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.tryHead with
                | Some token -> Parser.parseArgumentTokenValue token.Value
                | None -> failwith "F# Option can only be used with a single argument."
            ),            
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T option>

    static member Option<'T>(name: string, getDefaultValue: (unit -> 'T), ?description: string) =
        Option<'T>(
            name,
            getDefaultValue = getDefaultValue,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(name: string, ?description: string) =
        Option<'T>(
            name,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(aliases: string seq, getDefaultValue: (unit -> 'T), ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            getDefaultValue = getDefaultValue,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Option<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    // TODO: Add remaining Option overloads

    static member Argument<'T>(getDefaultValue: (unit -> 'T)) = 
        Argument<'T>(
            getDefaultValue = getDefaultValue
        ) :> Binding.IValueDescriptor<'T>

    static member Argument<'T>(name: string, ?description: string) = 
        Argument<'T>(
            name, 
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    static member Argument<'T>(name: string, getDefaultValue: (unit -> 'T), ?description: string) = 
        Argument<'T>(
            name,
            getDefaultValue = getDefaultValue,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    // TODO: Add remaining Argument overloads