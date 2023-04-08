﻿[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Builder
open System.CommandLine.Parsing

type private IC = System.CommandLine.Invocation.InvocationContext
let private def<'T> = Unchecked.defaultof<'T>

/// Parses a HandlerInput value using the InvocationContext.
let private parseInput<'V> (handlerInputs: HandlerInput list) (ctx: IC) (idx: int) =
    match handlerInputs[idx].Source with
    | ParsedOption o -> ctx.ParseResult.GetValueForOption<'V>(o :?> Option<'V>)
    | ParsedArgument a -> ctx.ParseResult.GetValueForArgument<'V>(a :?> Argument<'V>)
    | Context -> ctx |> unbox<'V>

type CommandSpec<'Inputs, 'Output> = 
    {
        Description: string
        Inputs: HandlerInput list
        Handler: 'Inputs -> 'Output
        Alias: string list
        SubCommands: System.CommandLine.Command list
        /// Registers extra inputs that can be parsed via the InvocationContext if more than 8 are required.
        ExtraInputs: HandlerInput list
    }
    static member Default = 
        { 
            Description = "My Command"
            Inputs = []
            Alias = []
            ExtraInputs = []
            Handler = def<unit -> 'Output> // Support unit -> 'Output handler by default
            SubCommands = []
        }

/// Contains shared operations for building a `RootCommand` or `Command`.
type BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>() = 

    let newHandler handler spec =
        {
            Description = spec.Description
            Inputs = spec.Inputs
            Alias = spec.Alias
            ExtraInputs = spec.ExtraInputs
            Handler = handler
            SubCommands = spec.SubCommands
        }

    member val CommandLineBuilder = CommandLineBuilder().UseDefaults() with get, set

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
    [<CustomOperation("setHandler")>]
    member this.SetHandler (spec: CommandSpec<'Inputs, 'Output>, handler: 'Inputs -> 'Output) =
        newHandler handler spec

    [<Obsolete("'setCommand' has been deprecated in favor of 'addCommand' or 'addCommands`.")>]
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

    [<CustomOperation("addAlias")>]
    member this.AddAlias (spec: CommandSpec<'Inputs, 'Output>, alias: string seq) =
        { spec with Alias = spec.Alias @ (alias |> Seq.toList) }

    [<CustomOperation("addAlias")>]
    member this.AddAlias (spec: CommandSpec<'Inputs, 'Output>, alias: string) =
        { spec with Alias = alias :: spec.Alias }

    /// Registers an additional input that can be manually parsed via the InvocationContext. (Use when more than 8 inputs are required.)
    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput<'Value>) =
        { spec with ExtraInputs = spec.ExtraInputs @ [ extraInput ] }

    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput seq) =
        { spec with ExtraInputs = spec.ExtraInputs @ (extraInput |> List.ofSeq) }

    [<CustomOperation("add")>]
    member this.Add(spec: CommandSpec<'Inputs, 'Output>, extraInput: HandlerInput<'Value> seq) =
        { spec with ExtraInputs = spec.ExtraInputs @ (extraInput |> Seq.cast |> List.ofSeq) }

    /// Sets general properties on the command.
    member this.SetGeneralProperties (spec: CommandSpec<'T, 'U>) (cmd: Command) = 
        cmd.Description <- spec.Description
        spec.Inputs
        |> Seq.iter (fun input ->
            match input.Source with
            | ParsedOption o -> cmd.AddOption o
            | ParsedArgument a -> cmd.AddArgument a
            | Context -> ()
        )
        spec.ExtraInputs
        |> Seq.iter (fun input ->
            match input.Source with
            | ParsedOption o -> cmd.AddOption o
            | ParsedArgument a -> cmd.AddArgument a
            | Context -> ()
        )

        spec.SubCommands |> List.iter cmd.AddCommand
        spec.Alias |> List.iter cmd.AddAlias
        cmd

    /// Sets a command handler that returns `unit`.
    member this.SetHandlerUnit (spec: CommandSpec<'Inputs, unit>) (cmd: Command) =
        let handler (args: obj) = 
            spec.Handler (args :?> 'Inputs)

        let getValue (ctx: IC) (idx: int) =
            parseInput spec.Inputs ctx idx

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Action(fun () -> 
                handler ()))
        | 01 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                handler (a)))
        | 02 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                handler (a, b)))
        | 03 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                handler (a, b, c)))
        | 04 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                handler (a, b, c, d)))
        | 05 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                handler (a, b, c, d, e)))
        | 06 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                handler (a, b, c, d, e, f)))
        | 07 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                handler (a, b, c, d, e, f, g)))
        | 08 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                let h: 'H = getValue ctx 7
                handler (a, b, c, d, e, f, g, h)))
        | _ -> raise (NotImplementedException("Only 8 inputs are supported."))
        cmd

    /// Sets a command handler that returns an `int` status code.
    member this.SetHandlerInt (spec: CommandSpec<'Inputs, int>) (cmd: Command) =
        let handler (args: obj) = 
            spec.Handler (args :?> 'Inputs)

        let getValue (ctx: IC) (idx: int) =
            parseInput spec.Inputs ctx idx

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                ctx.ExitCode <- handler ()))
        | 01 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                ctx.ExitCode <- handler (a)))
        | 02 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                ctx.ExitCode <- handler (a, b)))
        | 03 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                ctx.ExitCode <- handler (a, b, c)))
        | 04 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                ctx.ExitCode <- handler (a, b, c, d)))
        | 05 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                ctx.ExitCode <- handler (a, b, c, d, e)))
        | 06 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                ctx.ExitCode <- handler (a, b, c, d, e, f)))
        | 07 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                ctx.ExitCode <- handler (a, b, c, d, e, f, g)))
        | 08 -> cmd.SetHandler(Action<IC>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                let h: 'H = getValue ctx 7
                ctx.ExitCode <- handler (a, b, c, d, e, f, g, h)))
        | _ -> raise (NotImplementedException("Only 8 inputs are supported."))
        cmd

    /// Sets a command handler that returns a `Task`.
    member this.SetHandlerTask (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) (cmd: Command) =
        let handler (args: obj) = 
            task {
                return! spec.Handler (args :?> 'Inputs)
            }

        let getValue (ctx: IC) (idx: int) =
            parseInput spec.Inputs ctx idx

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Func<Task>(fun () -> 
                handler ()))
        | 01 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                handler (a)))
        | 02 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                handler (a, b)))
        | 03 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                handler (a, b, c)))
        | 04 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                handler (a, b, c, d)))
        | 05 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                handler (a, b, c, d, e)))
        | 06 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                handler (a, b, c, d, e, f)))
        | 07 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                handler (a, b, c, d, e, f, g)))
        | 08 -> cmd.SetHandler(Func<IC, Task>(fun ctx -> 
                let a: 'A = getValue ctx 0
                let b: 'B = getValue ctx 1
                let c: 'C = getValue ctx 2
                let d: 'D = getValue ctx 3
                let e: 'E = getValue ctx 4
                let f: 'F = getValue ctx 5
                let g: 'G = getValue ctx 6
                let h: 'H = getValue ctx 7
                handler (a, b, c, d, e, f, g, h)))
        | _ -> raise (NotImplementedException("Only 8 inputs are supported."))
        cmd

            
/// Builds a `System.CommandLine.Parsing.Parser`.
type RootCommandParserBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>() = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> unit) =
        subCommand this.CommandLineBuilder
        spec

    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> CommandLineBuilder) =
        this.CommandLineBuilder <- subCommand this.CommandLineBuilder
        spec

    /// Executes a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerUnit spec
        |> ignore
        this.CommandLineBuilder.Build()

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerInt spec
        |> ignore
        this.CommandLineBuilder.Build()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerTask spec
        |> ignore
        this.CommandLineBuilder.Build()

        
/// Builds and executes a `System.CommandLine.RootCommand`.
type RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args: string array) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> unit) =
        subCommand this.CommandLineBuilder
        spec

    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> CommandLineBuilder) =
        this.CommandLineBuilder <- subCommand this.CommandLineBuilder
        spec
        
    /// Executes a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerUnit spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).Invoke()

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerInt spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).Invoke()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetHandlerTask spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).InvokeAsync()
       

/// Builds a `System.CommandLine.Command`.
type CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(name: string) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()
    
    /// Returns a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) = 
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerUnit spec

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerInt spec

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetHandlerTask spec


/// Builds a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommandParser<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output> = 
    RootCommandParserBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>()

/// Builds and executes a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommand<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args: string array)= 
    RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(args)

/// Builds a `System.CommandLine.Command` using computation expression syntax.
let command<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output> (name: string) = 
    CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>(name)
