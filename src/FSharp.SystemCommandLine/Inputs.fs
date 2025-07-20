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

    /// Injects an `ActionContext` into the action which contains the `ParseResult` and a cancellation token.
    let context = 
        ActionInput<ActionContext>(Context)

    /// Creates a named option. Example: `option "--file-name"`
    let option<'T> (name: string) = 
        Option<'T>(name) |> ActionInput.OfOption

    /// Edits the underlying System.CommandLine.Option<'T>.
    let editOption (edit: Option<'T> -> unit) (input: ActionInput<'T>) = 
        match input.Source with
        | ParsedOption o -> o :?> Option<'T> |> edit
        | _ -> ()
        input

    /// Edits the underlying System.CommandLine.Argument<'T>.
    let editArgument (edit: Argument<'T> -> unit) (input: ActionInput<'T>) = 
        match input.Source with
        | ParsedArgument a -> a :?> Argument<'T> |> edit
        | _ -> ()
        input

    /// Adds one or more aliases to an option.
    let aliases (aliases: string seq) (input: ActionInput<'T>) = 
        input |> editOption (fun o -> aliases |> Seq.iter o.Aliases.Add)
        
    /// Adds an alias to an option.
    let alias (alias: string) (input: ActionInput<'T>) = 
        input |> editOption (fun o -> o.Aliases.Add alias)
                
    /// Sets the description of an option or argument.
    let description (description: string) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.Description <- description)
        |> editArgument (fun a -> a.Description <- description)

    /// An alias for `description` to set the description of the input.
    let desc = description

    /// Sets the default value of an option or argument.
    let defaultValue (defaultValue: 'T) (input: ActionInput<'T>) = 
        input
        |> editOption (fun o -> o.DefaultValueFactory <- (fun _ -> defaultValue))
        |> editArgument (fun a -> a.DefaultValueFactory <- (fun _ -> defaultValue))
        
    /// An alias for `defaultValue` to set the default value of an option or argument.
    let def = defaultValue

    /// Sets the default value factory of an option or argument.
    let defaultValueFactory (defaultValueFactory: Parsing.ArgumentResult -> 'T) (input: ActionInput<'T>) = 
        input
        |> editOption (fun o -> o.DefaultValueFactory <- defaultValueFactory)
        |> editArgument (fun a -> a.DefaultValueFactory <- defaultValueFactory)

    /// The name used in help output to describe the option or argument.
    let helpName (helpName: string) (input: ActionInput<'T>) =
        input
        |> editOption (fun o -> o.HelpName <- helpName)
        |> editArgument (fun a -> a.HelpName <- helpName)

    /// Marks an option as required.
    let required (input: ActionInput<'T>) = 
        input |> editOption (fun o -> o.Required <- true)

    /// Creates a named option of type `Option<'T option>` that defaults to `None`.
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

    /// Creates a named argument. Example: `argument "file-name"`
    let argument<'T> (name: string) = 
        let a = Argument<'T>(name)
        ActionInput.OfArgument<'T> a

    /// Creates a named argument of type `Argument<'T option>` that defaults to `None`.
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

    /// Adds a validator that validates the parsed value of an option or argument.
    let validate (validate: 'T -> Result<unit, string>) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> 
            o.Validators.Add(fun res -> 
                try
                    let value = res.GetValue<'T>(o.Name)
                    match validate value with
                    | Ok () -> ()
                    | Error err -> res.AddError(err)
                with 
                | :? InvalidOperationException ->
                    // res.GetValue<'T> will fire this ex when a customParser adds an error.
                    // In this case, the validation error will already be displayed.
                    () 
            )
        )
        |> editArgument (fun a -> 
            a.Validators.Add(fun res -> 
                try
                    let value = res.GetValue<'T>(a.Name)
                    match validate value with
                    | Ok () -> ()
                    | Error err -> res.AddError(err)
                with 
                | :? InvalidOperationException ->
                    // res.GetValue<'T> will fire this ex when a customParser adds an error.
                    // In this case, the validation error will already be displayed.
                    () 
            )
        )

    /// Validates that the file exists.
    let validateFileExists (input: ActionInput<System.IO.FileInfo>) = 
        input    
        |> validate (fun file -> 
            if file.Exists then Ok () 
            else Error $"File '{file.FullName}' does not exist."
        )

    /// Validates that the directory exists.
    let validateDirectoryExists (input: ActionInput<System.IO.DirectoryInfo>) = 
        input    
        |> validate (fun dir -> 
            if dir.Exists then Ok () 
            else Error $"Directory '{dir.FullName}' does not exist."
        )

    /// Adds a validator for the given `Parsing.SymbolResult`.
    let addValidator (validator: Parsing.SymbolResult -> unit) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.Validators.Add(validator))
        |> editArgument (fun a -> a.Validators.Add(validator))

    let acceptOnlyFromAmong (acceptedValues: string seq) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.AcceptOnlyFromAmong(acceptedValues |> Seq.toArray) |> ignore)
        |> editArgument (fun a -> a.AcceptOnlyFromAmong(acceptedValues |> Seq.toArray) |> ignore)

    /// Parses the input using a custom parser function.
    let customParser (parser: Parsing.ArgumentResult -> 'T) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.CustomParser <- parser)
        |> editArgument (fun a -> a.CustomParser <- parser)

    /// Creates a custom parser based on the result of the provided parser function.
    let tryParse (parser: Parsing.ArgumentResult -> Result<'T, string>) (input: ActionInput<'T>) = 
        input 
        |> customParser (fun argResult ->             
            match parser argResult with
            | Ok value -> 
                value
            | Error err -> 
                argResult.AddError(err)
                Unchecked.defaultof<'T>
        )

    /// Sets the arity of an option or argument.
    let arity (arity: ArgumentArity) (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.Arity <- arity)
        |> editArgument (fun a -> a.Arity <- arity)

    /// Sets a value that indicates whether multiple arguments are allowed for each option identifier token. (Defaults to 'false'.)
    let allowMultipleArgumentsPerToken (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.AllowMultipleArgumentsPerToken <- true)

    /// Hides an option or argument from the help output.
    let hidden (input: ActionInput<'T>) = 
        input 
        |> editOption (fun o -> o.Hidden <- true)
        |> editArgument (fun a -> a.Hidden <- true)

    /// Converts an `Option<'T>` to an `ActionInput<'T>` for usage with the command builders.
    let ofOption (o: Option<'T>) = 
        ActionInput.OfOption<'T> o

    /// Converts an `Argument<'T>` to an `ActionInput<'T>` for usage with the command builders.
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