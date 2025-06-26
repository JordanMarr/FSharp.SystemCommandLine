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

type ActionInputSource = 
    | ParsedOption of Option
    | ParsedArgument of Argument
    | Context

type ActionInput(source: ActionInputSource) = 
    member this.Source = source

type ActionInput<'T>(inputType: ActionInputSource) =
    inherit ActionInput(inputType)
    
    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    static member OfOption<'T>(o: Option<'T>) = o :> Option |> ParsedOption |> ActionInput<'T>
    
    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    static member OfArgument<'T>(a: Argument<'T>) = a :> Argument |> ParsedArgument |> ActionInput<'T>
        
    /// Gets the value of an Option or Argument from the Parser.
    member this.GetValue(parseResult: ParseResult) =
        match this.Source with
        | ParsedOption o -> o :?> Option<'T> |> parseResult.GetValue
        | ParsedArgument a -> a :?> Argument<'T> |> parseResult.GetValue
        | Context -> parseResult |> unbox<'T>

module Input = 

    let context = 
        ActionInput<ActionContext>(Context)

    let option<'T> (name: string) = 
        Option<'T>(name) |> ActionInput.OfOption

    let editOption (edit: Option<'T> -> unit) (hi: ActionInput<'T>) = 
        match hi.Source with
        | ParsedOption o -> o :?> Option<'T> |> edit
        | _ -> ()
        hi

    let editArgument (edit: Argument<'T> -> unit) (hi: ActionInput<'T>) = 
        match hi.Source with
        | ParsedArgument a -> a :?> Argument<'T> |> edit
        | _ -> ()
        hi

    let aliases (aliases: string seq) (hi: ActionInput<'T>) = 
        hi |> editOption (fun o -> aliases |> Seq.iter o.Aliases.Add)
        
    let alias (alias: string) (hi: ActionInput<'T>) = 
        hi |> editOption (fun o -> o.Aliases.Add alias)
                
    let desc (description: string) (hi: ActionInput<'T>) = 
        hi 
        |> editOption (fun o -> o.Description <- description)
        |> editArgument (fun a -> a.Description <- description)

    let defaultValue (defaultValue: 'T) (hi: ActionInput<'T>) = 
        hi
        |> editOption (fun o -> o.DefaultValueFactory <- (fun _ -> defaultValue))
        |> editArgument (fun a -> a.DefaultValueFactory <- (fun _ -> defaultValue))
        
    let def = defaultValue

    let defFactory (defaultValueFactory: Parsing.ArgumentResult -> 'T) (hi: ActionInput<'T>) = 
        hi
        |> editOption (fun o -> o.DefaultValueFactory <- defaultValueFactory)
        |> editArgument (fun a -> a.DefaultValueFactory <- defaultValueFactory)

    let required (hi: ActionInput<'T>) = 
        hi |> editOption (fun o -> o.Required <- true)

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
        ActionInput.OfOption<'T option> o

    let argument<'T> (name: string) = 
        let a = Argument<'T>(name)
        ActionInput.OfArgument<'T> a

    let argumentMaybe<'T> (name: string) = 
        let a = Argument<'T option>(name)
        a.DefaultValueFactory <- (fun _ -> None)
        a.CustomParser <- (fun argResult -> 
            match argResult.Tokens |> Seq.toList with
            | [] -> None
            | [ token ] -> MaybeParser.parseTokenValue token.Value
            | _ :: _ -> failwith "F# Option can only be used with a single argument."
        )
        ActionInput.OfArgument<'T option> a

    let ofOption (o: Option<'T>) = 
        ActionInput.OfOption<'T> o

    let ofArgument (a: Argument<'T>) = 
        ActionInput.OfArgument<'T> a

/// Creates CLI options and arguments to be passed as command `inputs`.
type Input = 

    /// Converts a System.CommandLine.Option<'T> for usage with the CommandBuilder.
    [<Obsolete("Use Input.ofOption instead")>]
    static member OfOption<'T>(o: Option<'T>) : ActionInput<'T> = 
        invalidOp "This method has been removed."

    /// Converts a System.CommandLine.Argument<'T> for usage with the CommandBuilder.
    [<Obsolete("Use Input.ofArgument instead")>]
    static member OfArgument<'T>(a: Argument<'T>) : ActionInput<'T> = 
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, configure: Option<'T> -> unit) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, configure: Option<'T> -> unit) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, configure: Argument<'T> -> unit) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."
    
    /// Creates a CLI option of type 'T with a default value.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(name: string, defaultValue: 'T, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T with a default value.
    [<Obsolete "Use Input.option instead.">]
    static member Option<'T>(aliases: string seq, defaultValue: 'T, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T that is required.
    [<Obsolete "Use Input.option + Input.required instead.">]
    static member OptionRequired<'T>(aliases: string seq, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."
        
    /// Creates a CLI option of type 'T that is required.
    [<Obsolete "Use Input.option + Input.required instead.">]
    static member OptionRequired<'T>(name: string, ?description: string) : ActionInput<'T> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(aliases: string seq, configure: Option<'T option> -> unit) : ActionInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option with the ability to manually configure the underlying properties.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(name: string, configure: Option<'T option> -> unit) : ActionInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option.
    [<Obsolete "Use Input.optionMaybe instead.">]
    static member OptionMaybe<'T>(aliases: string seq, ?description: string) : ActionInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI option of type 'T option.
    static member OptionMaybe<'T>(name: string, ?description: string) : ActionInput<'T option> =
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, ?description: string) : ActionInput<'T> = 
        invalidOp "This method has been removed."

    /// Creates a CLI argument of type 'T with a default value.
    [<Obsolete "Use Input.argument instead.">]
    static member Argument<'T>(name: string, defaultValue: 'T, ?description: string) : ActionInput<'T> = 
        invalidOp "This method has been removed."
    
    /// Creates a CLI argument of type 'T option.
    [<Obsolete "Use Input.argumentMaybe instead.">]
    static member ArgumentMaybe<'T>(name: string, ?description: string) : ActionInput<'T option> = 
        invalidOp "This method has been removed."

    /// Passes the `InvocationContext` to the handler.
    [<Obsolete "Use Input.context instead.">]
    static member Context() : ActionContext = 
        invalidOp "This method has been removed."