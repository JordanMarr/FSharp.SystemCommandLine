[<AutoOpen>]
module FSharp.SystemCommandLine.Inputs

open System
open System.CommandLine

module private MaybeParser = 
    /// Parses an argument token value. 
    /// TODO: Ideally, this should use the S.CL Arugment parser.
    let parseTokenValue<'T> (tokenValue: string) = 
        match typeof<'T> with
        | t when t = typeof<IO.DirectoryInfo> -> IO.DirectoryInfo(tokenValue) |> unbox<'T> |> Some
        | t when t = typeof<IO.FileInfo> -> IO.FileInfo(tokenValue) |> unbox<'T> |> Some
        | t when t = typeof<Uri> -> Uri(tokenValue) |> unbox<'T> |> Some
        | t -> Convert.ChangeType(tokenValue, t) :?> 'T |> Some

type HandlerInputSource = 
    | ParsedOption of Option
    | ParsedArgument of Argument
    | InjectedDependency

type HandlerInput(source: HandlerInputSource) = 
    member this.Source = source

type HandlerInput<'T>(inputType: HandlerInputSource) =
    inherit HandlerInput(inputType)
    static member OfOption<'T>(o: Option<'T>) = o :> Option |> ParsedOption |> HandlerInput<'T>
    static member OfArgument<'T>(a: Argument<'T>) = a :> Argument |> ParsedArgument |> HandlerInput<'T>


/// Creates CLI options and arguments to be passed as command `inputs`.
type Input = 
    
    /// Creates a CLI option of type 'T.
    static member Option<'T>(name: string, ?description: string) =
        Option<'T>(
            name,
            description = (description |> Option.defaultValue null)
        ) 
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T.
    static member Option<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            description = (description |> Option.defaultValue null)
        )
        |> HandlerInput.OfOption
    
    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(name: string, defaultValue: 'T, ?description: string) =
        Option<'T>(
            name,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        )
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(aliases: string seq, defaultValue: 'T, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        )
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T that is required.
    static member OptionRequired<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            aliases |> Seq.toArray,
            description = (description |> Option.defaultValue null),
            IsRequired = true
        )
        |> HandlerInput.OfOption
        
    /// Creates a CLI option of type 'T that is required.
    static member OptionRequired<'T>(name: string, ?description: string) =
        Input.OptionRequired<'T>([| name |], description |> Option.defaultValue null)

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(aliases: string seq, ?description: string) =
        let isBool = typeof<'T> = typeof<bool>
        Option<'T option>(
            aliases |> Seq.toArray,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.toList with
                | [] when isBool -> true |> unbox<'T> |> Some
                | [] -> None
                | [ token ] -> MaybeParser.parseTokenValue token.Value
                | _ :: _ -> failwith "F# Option can only be used with a single argument."
            ), 
            description = (description |> Option.defaultValue null),
            Arity = ArgumentArity(0, 1)
        )
        |> fun o -> o.SetDefaultValue(None); o
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(name: string, ?description: string) =
        Input.OptionMaybe<'T>([| name |], description |> Option.defaultValue null)

    /// Creates a CLI argument of type 'T.
    static member Argument<'T>(name: string, ?description: string) = 
        Argument<'T>(
            name, 
            description = (description |> Option.defaultValue null)
        ) 
        |> HandlerInput.OfArgument

    /// Creates a CLI argument of type 'T with a default value.
    static member Argument<'T>(name: string, defaultValue: 'T, ?description: string) = 
        Argument<'T>(
            name,
            getDefaultValue = (fun () -> defaultValue),
            description = (description |> Option.defaultValue null)
        )
        |> HandlerInput.OfArgument
    
    /// Creates a CLI argument of type 'T option.
    static member ArgumentMaybe<'T>(name: string, ?description: string) = 
        Argument<'T option>(
            name,
            parse = (fun argResult -> 
                match argResult.Tokens |> Seq.toList with
                | [] -> None
                | [ token ] -> MaybeParser.parseTokenValue token.Value
                | _ :: _ -> failwith "F# Option can only be used with a single argument."
            ), 
            description = (description |> Option.defaultValue null),
            isDefault = true
        )
        |> HandlerInput.OfArgument

    /// Creates an injected dependency input.
    static member InjectedDependency<'T>() = 
        HandlerInput<'T>(InjectedDependency)