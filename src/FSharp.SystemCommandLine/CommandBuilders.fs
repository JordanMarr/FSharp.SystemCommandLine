[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Parsing

let private def<'T> = Unchecked.defaultof<'T>

/// Gets the underlying value of a `HandlerInput` based on its source.
let private parseInput<'V> (handlerInput: HandlerInput) (pr: ParseResult) (cancelToken: CancellationToken) =
    match handlerInput.Source with
    | ParsedOption o -> pr.GetValue<'V>(o :?> Option<'V>)
    | ParsedArgument a -> pr.GetValue<'V>(a :?> Argument<'V>)
    | Context -> { ParseResult = pr; CancellationToken = cancelToken } |> unbox<'V>

/// Converts up to 8 handler inputs into a tuple of the specified action input type.
let private inputsToTuple<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Input> (pr: ParseResult) (ct: CancellationToken) (inputs: HandlerInput list) =
    match inputs.Length with
    | 0 -> () |> unbox<'Input>
    | 1 -> 
        let a = parseInput<'A> inputs[0] pr ct
        a |> unbox<'Input>
    | 2 -> 
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        (a, b) |> unbox<'Input>
    | 3 ->
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        (a, b, c) |> unbox<'Input>
    | 4 ->
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        let d = parseInput<'D> inputs[3] pr ct
        (a, b, c, d) |> unbox<'Input>
    | 5 -> 
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        let d = parseInput<'D> inputs[3] pr ct
        let e = parseInput<'E> inputs[4] pr ct
        (a, b, c, d, e) |> unbox<'Input>
    | 6 -> 
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        let d = parseInput<'D> inputs[3] pr ct
        let e = parseInput<'E> inputs[4] pr ct
        let f = parseInput<'F> inputs[5] pr ct
        (a, b, c, d, e, f) |> unbox<'Input>
    | 7 ->
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        let d = parseInput<'D> inputs[3] pr ct
        let e = parseInput<'E> inputs[4] pr ct
        let f = parseInput<'F> inputs[5] pr ct
        let g = parseInput<'G> inputs[6] pr ct
        (a, b, c, d, e, f, g) |> unbox<'Input>
    | 8 ->
        let a = parseInput<'A> inputs[0] pr ct
        let b = parseInput<'B> inputs[1] pr ct
        let c = parseInput<'C> inputs[2] pr ct
        let d = parseInput<'D> inputs[3] pr ct
        let e = parseInput<'E> inputs[4] pr ct
        let f = parseInput<'F> inputs[5] pr ct
        let g = parseInput<'G> inputs[6] pr ct
        let h = parseInput<'H> inputs[7] pr ct
        (a, b, c, d, e, f, g, h) |> unbox<'Input>
    | _ -> 
        invalidOp "Only 8 inputs are supported."
        
/// Adds global options to a command, ensuring they are recursive.
let private addGlobalOptionsToCommand (globalOptions: HandlerInput list) (cmd: Command) =
    for g in globalOptions do
        match g.Source with
        | ParsedOption o -> 
            o.Recursive <- true // Ensure global options are recursive
            cmd.Add(o)
        | ParsedArgument _ -> () // cmd.Add(a) // TODO: Should arguments be added globally?
        | Context -> () 
    cmd

type CommandSpec<'Inputs, 'Output> = 
    {
        Description: string
        Inputs: HandlerInput list
        GlobalInputs: HandlerInput list
        Handler: 'Inputs -> 'Output
        Aliases: string list
        SubCommands: System.CommandLine.Command list
        /// Registers extra inputs that can be parsed via the InvocationContext if more than 8 are required.
        ExtraInputs: HandlerInput list
    }
    static member Default = 
        { 
            Description = "My Command"
            Inputs = []
            GlobalInputs = []
            Aliases = []
            ExtraInputs = []
            Handler = def<unit -> 'Output> // Support unit -> 'Output handler by default
            SubCommands = []
        }

/// Contains shared operations for building a `rootCommand`, `command` or `commandLineConfiguration` CE.
type BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>() = 

    let newHandler handler spec =
        {
            Description = spec.Description
            Inputs = spec.Inputs
            GlobalInputs = spec.GlobalInputs
            Aliases = spec.Aliases
            ExtraInputs = spec.ExtraInputs
            Handler = handler
            SubCommands = spec.SubCommands
        }

    //member val CommandLineBuilder = CommandLineBuilder().UseDefaults() with get, set
    member val CommandLineConfiguration = new CommandLineConfiguration(new RootCommand()) with get, set

    member this.Yield _ =
        CommandSpec<unit, 'Output>.Default 

    member this.Zero _ = 
        CommandSpec<unit, 'Output>.Default

    /// A description that will be displayed to the command line user.
    [<CustomOperation("description")>]
    member this.Description (spec: CommandSpec<'T, 'U>, description) =
        { spec with Description = description }
    
    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, a: HandlerInput<'A>) =
        { newHandler def<'A -> 'Output> spec with Inputs = [ a ] }
    
    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>)) =
        { newHandler def<'A * 'B -> 'Output> spec with Inputs = [ a; b ] }

    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>)) =
        { newHandler def<'A * 'B * 'C -> 'Output> spec with Inputs = [ a; b; c ] }
        
    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>, d: HandlerInput<'D>)) =
        { newHandler def<'A * 'B * 'C * 'D -> 'Output> spec with Inputs = [ a; b; c; d ] }
            
    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>, d: HandlerInput<'D>, e: HandlerInput<'E>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E -> 'Output> spec with Inputs = [ a; b; c; d; e ] }

    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>, d: HandlerInput<'D>, e: HandlerInput<'E>, f: HandlerInput<'F>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F -> 'Output> spec with Inputs = [ a; b; c; d; e; f ] }

    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>, d: HandlerInput<'D>, e: HandlerInput<'E>, f: HandlerInput<'F>, g: HandlerInput<'G>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g ] }

    /// A tuple of inputs (max. 8) that must exactly match the handler function inputs.
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HandlerInput<'A>, b: HandlerInput<'B>, c: HandlerInput<'C>, d: HandlerInput<'D>, e: HandlerInput<'E>, f: HandlerInput<'F>, g: HandlerInput<'G>, h: HandlerInput<'H>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h ] }

    /// Sets a handler function that takes a tuple of inputs (max. 8). NOTE: This must be set after the inputs.
    [<CustomOperation("setHandler"); Obsolete("Please use `setAction` instead.")>]
    member this.SetHandler (spec: CommandSpec<'Inputs, 'Output>, handler: 'Inputs -> 'Output) =
        newHandler handler spec

    /// Sets an action function that takes a tuple of inputs (max. 8). NOTE: This must be set after the inputs.
    [<CustomOperation("setAction")>]
    member this.SetAction (spec: CommandSpec<'Inputs, 'Output>, action: 'Inputs -> 'Output) =
        newHandler action spec

    /// Sets an empty handler action that does nothing. This is used when no action is required, such as when only sub-commands are defined.
    [<CustomOperation("noAction")>]
    member this.NoAction (spec: CommandSpec<'Inputs, 'Output>) =
        newHandler (fun () -> ()) spec

    [<CustomOperation("addGlobalOption")>]
    member this.AddGlobalOption (spec: CommandSpec<'Inputs, 'Output>, globalInput: HandlerInput) =
        { spec with GlobalInputs = spec.GlobalInputs @ [ globalInput ] }

    [<CustomOperation("addGlobalOptions")>]
    member this.AddGlobalOptions (spec: CommandSpec<'Inputs, 'Output>, globalInputs: HandlerInput seq) =
        { spec with GlobalInputs = spec.GlobalInputs @ (globalInputs |> List.ofSeq) }

    [<Obsolete("'setCommand' has been deprecated in favor of 'addCommand' or 'addCommands'.")>]
    [<CustomOperation("setCommand")>] 
    member this.SetCommand (spec: CommandSpec<'Inputs, 'Output>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }

    /// Adds a sub-command.
    [<CustomOperation("addCommand")>]
    member this.AddCommand (spec: CommandSpec<'Inputs, 'Output>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }

    /// Adds sub-commands.
    [<CustomOperation("addCommands")>]
    member this.AddCommands (spec: CommandSpec<'Inputs, 'Output>, subCommands: System.CommandLine.Command seq) =
        { spec with SubCommands = spec.SubCommands @ (subCommands |> Seq.toList) }

    /// Adds an alias to the command.
    [<CustomOperation("addAlias")>]
    member this.AddAlias (spec: CommandSpec<'Inputs, 'Output>, alias: string) =
        { spec with Aliases = alias :: spec.Aliases }

    /// Adds aliases to the command.
    [<CustomOperation("addAliases")>]
    member this.AddAliases (spec: CommandSpec<'Inputs, 'Output>, aliases: string seq) =
        { spec with Aliases = spec.Aliases @ (aliases |> Seq.toList) }

    [<Obsolete("'add' has been deprecated in favor of 'addInput'.")>]
    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput<'Value>) =
        { spec with ExtraInputs = spec.ExtraInputs @ [ extraInput ] }

    [<Obsolete("'add' has been deprecated in favor of 'addInputs'.")>]
    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput seq) =
        { spec with ExtraInputs = spec.ExtraInputs @ (extraInput |> List.ofSeq) }

    [<Obsolete("'add' has been deprecated in favor of 'addInputs'.")>]
    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput<'Value> seq) =
        { spec with ExtraInputs = spec.ExtraInputs @ (extraInput |> Seq.cast |> List.ofSeq) }

    /// Adds an extra input (when more than 8 inputs are required).
    [<CustomOperation("addInput")>]
    member this.AddInput(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput) =
        { spec with ExtraInputs = spec.ExtraInputs @ [ extraInput ] }

    /// Adds extra inputs (when more than 8 inputs are required).
    [<CustomOperation("addInputs")>]
    member this.AddInputs(spec: CommandSpec<'Inputs, 'Output>, extraInputs: HandlerInput seq) =
        { spec with ExtraInputs = spec.ExtraInputs @ (extraInputs |> List.ofSeq) }

    /// Sets general properties on the command.
    member this.SetGeneralProperties (spec: CommandSpec<'T, 'U>) (cmd: Command) = 
        cmd.Description <- spec.Description
        spec.Inputs
        |> Seq.iter (fun input ->
            match input.Source with
            | ParsedOption o -> cmd.Add o
            | ParsedArgument a -> cmd.Add a
            | Context -> ()
        )
        spec.ExtraInputs
        |> Seq.iter (fun input ->
            match input.Source with
            | ParsedOption o -> cmd.Add o
            | ParsedArgument a -> cmd.Add a
            | Context -> ()
        )
        spec.SubCommands |> List.iter cmd.Add
        spec.Aliases |> List.iter cmd.Aliases.Add
        cmd

    /// Sets a command handler that returns `unit`.
    member this.SetHandlerUnit (spec: CommandSpec<'Inputs, unit>) (cmd: Command) =
        cmd.SetAction(fun pr -> 
            let input = inputsToTuple<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Inputs> pr CancellationToken.None spec.Inputs
            spec.Handler input
        )
        cmd

    /// Sets a command handler that returns an `int` status code.
    member this.SetHandlerInt (spec: CommandSpec<'Inputs, int>) (cmd: Command) =
        cmd.SetAction(fun pr -> 
            let input = inputsToTuple<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Inputs> pr CancellationToken.None spec.Inputs
            spec.Handler input
        )
        cmd

    /// Sets a command handler for an action function that returns a `Task<unit>`.
    member this.SetHandlerTask (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) (cmd: Command) =
        cmd.SetAction(Func<ParseResult, CancellationToken, Task>(fun pr ct -> 
            let input = inputsToTuple<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Inputs> pr ct spec.Inputs
            spec.Handler input
        ))
        cmd

    /// Sets a command handler for an action function that returns a `Task<int>`.
    member this.SetHandlerTaskInt (spec: CommandSpec<'Inputs, Task<int>>) (cmd: Command) =
        cmd.SetAction(Func<ParseResult, CancellationToken, Task<int>>(fun pr ct -> 
            let input = inputsToTuple<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Inputs> pr ct spec.Inputs
            spec.Handler input
        ))
        cmd

            
/// Builds a `System.CommandLineConfiguration` that can be passed to the `CommandLineParser.Parse` static method.
type CommandLineConfigurationBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>() = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    [<CustomOperation("usePipeline"); Obsolete("Please use `configure` instead.")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> unit) =
        subCommand this.CommandLineConfiguration
        spec

    [<CustomOperation("usePipeline"); Obsolete("Please use `configure` instead.")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> CommandLineConfiguration) =
        this.CommandLineConfiguration <- subCommand this.CommandLineConfiguration
        spec
        
    [<CustomOperation("configure")>]
    member this.Configure (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> unit) =
        subCommand this.CommandLineConfiguration
        spec

    [<CustomOperation("configure")>]
    member this.Configure (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> CommandLineConfiguration) =
        this.CommandLineConfiguration <- subCommand this.CommandLineConfiguration
        spec

    /// Executes a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =
        this.CommandLineConfiguration.RootCommand
        |> this.SetGeneralProperties spec
        |> this.SetHandlerUnit spec
        |> addGlobalOptionsToCommand spec.GlobalInputs
        |> ignore
        this.CommandLineConfiguration

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        this.CommandLineConfiguration.RootCommand
        |> this.SetGeneralProperties spec
        |> this.SetHandlerInt spec
        |> addGlobalOptionsToCommand spec.GlobalInputs
        |> ignore
        this.CommandLineConfiguration

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        this.CommandLineConfiguration.RootCommand
        |> this.SetGeneralProperties spec
        |> this.SetHandlerTask spec
        |> addGlobalOptionsToCommand spec.GlobalInputs
        |> ignore
        this.CommandLineConfiguration

        
/// Builds and executes a `System.CommandLine.RootCommand`.
type RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args: string array) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    [<CustomOperation("usePipeline"); Obsolete("Please use `configure` instead.")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> unit) =
        subCommand this.CommandLineConfiguration
        spec

    [<CustomOperation("usePipeline"); Obsolete("Please use `configure` instead.")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineConfiguration -> CommandLineConfiguration) =
        this.CommandLineConfiguration <- subCommand this.CommandLineConfiguration
        spec
        
    /// Allows modification of the CommandLineConfiguration.
    [<CustomOperation("configure")>]
    member this.Configure (spec: CommandSpec<'Inputs, 'Output>, configure: CommandLineConfiguration -> unit) =
        configure this.CommandLineConfiguration
        spec

    /// Allows modification of the CommandLineConfiguration.
    [<CustomOperation("configure")>]
    member this.Configure (spec: CommandSpec<'Inputs, 'Output>, configure: CommandLineConfiguration -> CommandLineConfiguration) =
        this.CommandLineConfiguration <- configure this.CommandLineConfiguration
        spec
        
    /// Executes a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =
        let rootCommand = 
            this.CommandLineConfiguration.RootCommand
            |> this.SetGeneralProperties spec
            |> this.SetHandlerUnit spec
            |> addGlobalOptionsToCommand spec.GlobalInputs
        
        CommandLineParser.Parse(rootCommand, args).Invoke()

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        let rootCommand = 
            this.CommandLineConfiguration.RootCommand
            |> this.SetGeneralProperties spec
            |> this.SetHandlerInt spec
            |> addGlobalOptionsToCommand spec.GlobalInputs
        
        CommandLineParser.Parse(rootCommand, args).Invoke()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<unit>>) =
        let rootCommand = 
            this.CommandLineConfiguration.RootCommand
            |> this.SetGeneralProperties spec
            |> this.SetHandlerTask spec
            |> addGlobalOptionsToCommand spec.GlobalInputs
        
        CommandLineParser.Parse(rootCommand, args).InvokeAsync()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<int>>) =
        let rootCommand = 
            this.CommandLineConfiguration.RootCommand
            |> this.SetGeneralProperties spec
            |> this.SetHandlerTaskInt spec
            |> addGlobalOptionsToCommand spec.GlobalInputs
        
        CommandLineParser.Parse(rootCommand, args).InvokeAsync()
       

/// Builds a `System.CommandLine.Command`.
type CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(name: string) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    /// Returns a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) = 
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerUnit spec
        |> addGlobalOptionsToCommand spec.GlobalInputs

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerInt spec
        |> addGlobalOptionsToCommand spec.GlobalInputs

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerTask spec
        |> addGlobalOptionsToCommand spec.GlobalInputs


/// Builds a `System.CommandLineConfiguration` that can be passed to the `CommandLineParser.Parse` static method using computation expression syntax.
let commandLineConfiguration<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output> = 
    CommandLineConfigurationBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()

/// Builds and executes a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommand<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args: string array)= 
    RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args)

/// Builds a `System.CommandLine.Command` using computation expression syntax.
let command<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output> (name: string) = 
    CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(name)
