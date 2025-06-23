namespace FSharp.SystemCommandLine

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

/// A custom action context that contains the `ParseResult` and a cancellation token.
type ActionContext = 
    {
        ParseResult: ParseResult
        CancellationToken: System.Threading.CancellationToken
    }
    static member Create(parseResult: ParseResult) = 
        {   
            ParseResult = parseResult
            CancellationToken = System.Threading.CancellationToken.None 
        }

type HandlerInputSource = 
    | ParsedOption of Option
    | ParsedArgument of Argument
    | Context

type HandlerInput(source: HandlerInputSource) = 
    member this.Source = source

type HandlerInput<'T>(inputType: HandlerInputSource) =
    inherit HandlerInput(inputType)
    
    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    static member OfOption<'T>(o: Option<'T>) = o :> Option |> ParsedOption |> HandlerInput<'T>
    
    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    static member OfArgument<'T>(a: Argument<'T>) = a :> Argument |> ParsedArgument |> HandlerInput<'T>
        
    /// Gets the value of an Option or Argument from the Parser.
    member this.GetValue(parseResult: ParseResult) =
        match this.Source with
        | ParsedOption o -> o :?> Option<'T> |> parseResult.GetValue
        | ParsedArgument a -> a :?> Argument<'T> |> parseResult.GetValue
        | Context -> parseResult |> unbox<'T>

module Input = 

    let context = 
        HandlerInput<ActionContext>(Context)

    let option<'T> (name: string) = 
        Option<'T>(name) |> HandlerInput.OfOption

    let aliases (aliases: string seq) (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> 
                aliases |> Seq.iter o.Aliases.Add
                hi
            | _ -> hi

    let alias (alias: string) (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> o.Aliases.Add alias; hi
            | _ -> hi
                
    let desc (description: string) (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> o.Description <- description; o :?> Option<'T> |> HandlerInput.OfOption<'T>
            | ParsedArgument a -> a.Description <- description; a :?> Argument<'T> |> HandlerInput.OfArgument<'T>
            | Context -> hi

    let defaultValue (defaultValue: 'T) (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> 
                let o = o :?> Option<'T>
                o.DefaultValueFactory <- (fun _ -> defaultValue)
                HandlerInput.OfOption<'T> o
            | ParsedArgument a -> 
                let a = a :?> Argument<'T>
                a.DefaultValueFactory <- (fun _ -> defaultValue)
                HandlerInput.OfArgument<'T> a
            | Context -> hi

    let def = defaultValue

    let defFactory (defaultValueFactory: Parsing.ArgumentResult -> 'T) (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> 
                let o = o :?> Option<'T>
                o.DefaultValueFactory <- defaultValueFactory
                HandlerInput.OfOption<'T> o
            | ParsedArgument a -> 
                let a = a :?> Argument<'T>
                a.DefaultValueFactory <- defaultValueFactory
                HandlerInput.OfArgument<'T> a
            | Context -> hi

    let required (hi: HandlerInput<'T>) = 
        hi.Source 
        |> function 
            | ParsedOption o -> 
                let o = o :?> Option<'T>
                o.Required <- true
                HandlerInput.OfOption<'T> o
            | _ -> hi

    let optionMaybe<'T> (name: string) = 
        let o = Option<'T option>(name, aliases = [||])
        let isBool = typeof<'T> = typeof<bool>
        o.CustomParser <- (fun result -> 
            match result.Tokens |> Seq.toList with
            | [] when isBool -> true |> unbox<'T> |> Some
            | [] -> None
            | [ token ] -> MaybeParser.parseTokenValue token.Value
            | _ :: _ -> failwith "F# Option can only be used with a single argument."
        )
        o.Arity <- ArgumentArity(0, 1)
        o.DefaultValueFactory <- (fun _ -> None)
        HandlerInput.OfOption<'T option> o

    let argument<'T> (name: string) = 
        let a = Argument<'T>(name)
        HandlerInput.OfArgument<'T> a

    let argumentMaybe<'T> (name: string) = 
        let a = Argument<'T option>(name)
        a.DefaultValueFactory <- (fun _ -> None)
        a.CustomParser <- (fun argResult -> 
            match argResult.Tokens |> Seq.toList with
            | [] -> None
            | [ token ] -> MaybeParser.parseTokenValue token.Value
            | _ :: _ -> failwith "F# Option can only be used with a single argument."
        )
        HandlerInput.OfArgument<'T option> a

    let ofOption (o: Option<'T>) = 
        HandlerInput.OfOption<'T> o

    let ofArgument (a: Argument<'T>) = 
        HandlerInput.OfArgument<'T> a

/// Creates CLI options and arguments to be passed as command `inputs`.
type Input = 

    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    [<Obsolete("Use Input.ofOption instead")>]
    static member OfOption<'T>(o: Option<'T>) : HandlerInput<'T> = 
        invalidOp "This method has been removed."

    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    [<Obsolete("Use Input.ofArgument instead")>]
    static member OfArgument<'T>(a: Argument<'T>) : HandlerInput<'T> = 
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, configure: Option<'T> -> unit) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, configure: Option<'T> -> unit) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, configure: Argument<'T> -> unit) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."
    
    /// Creates a CLI option of type 'T with a default value.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, defaultValue: 'T, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with a default value.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, defaultValue: 'T, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T that is required.
    [<Obsolete "Use Input.option + Input.required instead.">]
    static member OptionRequired<'T>(aliases: string seq, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."
        
    /// Creates a CLI option of type 'T that is required.
    [<Obsolete "Use Input.option + Input.required instead.">]
    static member OptionRequired<'T>(name: string, ?description: string) : HandlerInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(aliases: string seq, configure: Option<'T option> -> unit) : HandlerInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(name: string, configure: Option<'T option> -> unit) : HandlerInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(aliases: string seq, ?description: string) : HandlerInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(name: string, ?description: string) : HandlerInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, ?description: string) : HandlerInput<'T> = 
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T with a default value.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, defaultValue: 'T, ?description: string) : HandlerInput<'T> = 
        invalidOp "This method has been removed."
    
    /// Creates a CLI argument of type 'T option.
    [<Obsolete "Use Input.argumentMaybe instead.">]
    static member ArgumentMaybe<'T>(name: string, ?description: string) : HandlerInput<'T option> = 
        invalidOp "This method has been removed."

    /// Passes the `InvocationContext` to the handler.
    [<Obsolete "Use Input.context instead.">]
    static member Context() : ActionContext = 
        invalidOp "This method has been removed."