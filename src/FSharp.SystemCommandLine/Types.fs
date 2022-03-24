[<AutoOpen>]
module FSharp.SystemCommandLine.Types

open System
open System.CommandLine

module private Parser = 
    /// Parses an argument token value. 
    /// TODO: Ideally, this should use the S.CL Arugment parser.
    let parseTokenValue<'T> (tokenValue: string) = 
        match typeof<'T> with
        | t when t = typeof<IO.DirectoryInfo> -> IO.DirectoryInfo(tokenValue) |> box :?> 'T |> Some
        | t when t = typeof<IO.FileInfo> -> IO.FileInfo(tokenValue) |> box :?> 'T |> Some
        | t -> Convert.ChangeType(tokenValue, t) :?> 'T |> Some

/// Creates CLI options and arguments to be passed as command `inputs`.
type Input = 

    /// Creates a CLI option of type 'T.
    static member Option<'T>(name: string, ?description: string) =
        Option<'T>(
            name,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    /// Creates a CLI option of type 'T.
    static member Option<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(name: string, defaultValue: 'T, ?description: string) =
        Option<'T>(
            name,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(aliases: string seq, defaultValue: 'T, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(name: string, ?description: string) =
        Option<'T option>(
            name,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.tryHead with
                | Some token -> Parser.parseTokenValue token.Value
                | None -> failwith "F# Option can only be used with a single argument."
            ),            
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T option>

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(aliases: string seq, ?description: string) =
        Option<'T option>(
            aliases |> Seq.toArray,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.tryHead with
                | Some token -> Parser.parseTokenValue token.Value
                | None -> failwith "F# Option can only be used with a single argument."
            ),            
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T option>

    /// Creates a CLI argument of type 'T.
    static member Argument<'T>(name: string, ?description: string) = 
        Argument<'T>(
            name, 
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>

    /// Creates a CLI argument of type 'T with a default value.
    static member Argument<'T>(name: string, defaultValue: 'T, ?description: string) = 
        Argument<'T>(
            name,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T>
    
    /// Creates a CLI argument of type 'T option.
    static member ArgumentMaybe<'T>(name: string, ?description: string) = 
        Argument<'T option>(
            name,
            parse = (fun argResult -> 
                match argResult.Tokens |> Seq.tryHead with
                | Some token -> Parser.parseTokenValue token.Value
                | None -> failwith "F# Option can only be used with a single argument."
            ),   
            description = (description |> Option.defaultValue null)
        ) :> Binding.IValueDescriptor<'T option>
