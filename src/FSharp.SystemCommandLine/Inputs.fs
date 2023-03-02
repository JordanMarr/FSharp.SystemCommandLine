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
    | Context
    | Cancel

type HandlerInput(source: HandlerInputSource) = 
    member this.Source = source

type HandlerInput<'T>(inputType: HandlerInputSource) =
    inherit HandlerInput(inputType)
    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    static member OfOption<'T>(o: Option<'T>) = o :> Option |> ParsedOption |> HandlerInput<'T>
    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    static member OfArgument<'T>(a: Argument<'T>) = a :> Argument |> ParsedArgument |> HandlerInput<'T>
    member this.GetValue(ctx: System.CommandLine.Invocation.InvocationContext) =
        match this.Source with
        | ParsedOption o -> o :?> Option<'T> |> ctx.ParseResult.GetValue
        | ParsedArgument a -> a :?> Argument<'T> |> ctx.ParseResult.GetValue
        | Context -> ctx |> unbox<'T>
        | Cancel -> failwith "Invalid operation"
        
let private applyConfiguration configure a = 
    configure a; a

/// Creates CLI options and arguments to be passed as command `inputs`.
type Input = 

    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    static member OfOption<'T>(o: Option<'T>) = 
        HandlerInput.OfOption o

    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    static member OfArgument<'T>(a: Argument<'T>) = 
        HandlerInput.OfArgument a
            
    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    static member Option<'T>(name: string, configure) =
        Option<'T>(name) 
        |> applyConfiguration configure
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    static member Option<'T>(aliases: string seq, configure) =
        Option<'T>(Seq.toArray aliases) 
        |> applyConfiguration configure
        |> HandlerInput.OfOption

    /// Creates a CLI argument of type 'T with the ability to manually configure the underlying properties.
    static member Argument<'T>(name: string, configure) =
        Argument<'T>(name) 
        |> applyConfiguration configure
        |> HandlerInput.OfArgument

    /// Creates a CLI option of type 'T.
    static member Option<'T>(name: string, ?description: string) =
        Option<'T>(
            name,
            ?description = description
        ) 
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T.
    static member Option<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            Seq.toArray aliases,
            ?description = description
        )
        |> HandlerInput.OfOption
    
    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(name: string, defaultValue: 'T, ?description: string) =
        Input.Option<'T>(name, fun o -> 
            o.SetDefaultValue(defaultValue)
            Option.iter (fun desc -> o.Description <- desc) description
        )

    /// Creates a CLI option of type 'T with a default value.
    static member Option<'T>(aliases: string seq, defaultValue: 'T, ?description: string) =
        Input.Option<'T>(aliases, fun o -> 
            o.SetDefaultValue(defaultValue)
            Option.iter (fun desc -> o.Description <- desc) description
        )

    /// Creates a CLI option of type 'T that is required.
    static member OptionRequired<'T>(aliases: string seq, ?description: string) =
        Option<'T>(
            Seq.toArray aliases,
            ?description = description,
            IsRequired = true
        )
        |> HandlerInput.OfOption
        
    /// Creates a CLI option of type 'T that is required.
    static member OptionRequired<'T>(name: string, ?description: string) =
        Input.OptionRequired<'T>([| name |], ?description = description)

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    static member OptionMaybe<'T>(aliases: string seq, configure) =
        let isBool = typeof<'T> = typeof<bool>
        Option<'T option>(
            Seq.toArray aliases,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.toList with
                | [] when isBool -> true |> unbox<'T> |> Some
                | [] -> None
                | [ token ] -> MaybeParser.parseTokenValue token.Value
                | _ :: _ -> failwith "F# Option can only be used with a single argument."
            ), 
            Arity = ArgumentArity(0, 1)
        )
        |> fun o -> o.SetDefaultValue(None); o
        |> applyConfiguration configure
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    static member OptionMaybe<'T>(name: string, configure) =
        Input.OptionMaybe<'T>([|name|], configure = configure)

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(aliases: string seq, ?description: string) =
        let isBool = typeof<'T> = typeof<bool>
        Option<'T option>(
            Seq.toArray aliases,
            parseArgument = (fun argResult -> 
                match argResult.Tokens |> Seq.toList with
                | [] when isBool -> true |> unbox<'T> |> Some
                | [] -> None
                | [ token ] -> MaybeParser.parseTokenValue token.Value
                | _ :: _ -> failwith "F# Option can only be used with a single argument."
            ), 
            ?description = description,
            Arity = ArgumentArity(0, 1)
        )
        |> fun o -> o.SetDefaultValue(None); o
        |> HandlerInput.OfOption

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(name: string, ?description) =
        Input.OptionMaybe<'T>([| name |], ?description = description)

    /// Creates a CLI argument of type 'T.
    static member Argument<'T>(name: string, ?description: string) = 
        Argument<'T>(
            name, 
            ?description = description
        ) 
        |> HandlerInput.OfArgument

    /// Creates a CLI argument of type 'T with a default value.
    static member Argument<'T>(name: string, defaultValue: 'T, ?description: string) = 
        Input.Argument<'T>(name, fun a ->
            a.SetDefaultValue(defaultValue)
            Option.iter (fun desc -> a.Description <- desc) description
        )
    
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
            ?description = description
        )
        |> fun o -> o.SetDefaultValue(None); o
        |> HandlerInput.OfArgument

    /// Passes the `InvocationContext` to the handler.
    static member Context() = 
        HandlerInput<Invocation.InvocationContext>(Context)

    /// Passes a `CancellationToken` to the handler.
    static member Cancel() = 
        HandlerInput<System.Threading.CancellationToken>(Cancel)