[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Builder
open System.CommandLine.Parsing

type private HI<'T> = HandlerInput<'T>
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
        SubCommands: System.CommandLine.Command list
    }
    static member Default = 
        { 
            Description = "My Command"
            Inputs = []
            Handler = def<unit -> 'Output> // Support unit -> 'Output handler by default
            SubCommands = []
        }

/// Contains shared operations for building a `RootCommand` or `Command`.
type BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'Output>() = 

    let newHandler handler spec =
        {
            Description = spec.Description
            Inputs = spec.Inputs
            Handler = handler
            SubCommands = spec.SubCommands
        }

    member val CommandLineBuilder = CommandLineBuilder().UseDefaults() with get, set

    member this.Yield _ =
        CommandSpec<unit, 'Output>.Default 

    member this.Zero _ = 
        CommandSpec<unit, 'Output>.Default

    [<CustomOperation("description")>]
    member this.Description (spec: CommandSpec<'T, 'U>, description) =
        { spec with Description = description }
    
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, a: HI<'A>) =
        { newHandler def<'A -> 'Output> spec with Inputs = [ a ] }
    
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>)) =
        { newHandler def<'A * 'B -> 'Output> spec with Inputs = [ a; b ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>)) =
        { newHandler def<'A * 'B * 'C -> 'Output> spec with Inputs = [ a; b; c ] }
        
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>)) =
        { newHandler def<'A * 'B * 'C * 'D -> 'Output> spec with Inputs = [ a; b; c; d ] }
            
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E -> 'Output> spec with Inputs = [ a; b; c; d; e ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F -> 'Output> spec with Inputs = [ a; b; c; d; e; f ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h ] }

    [<CustomOperation("setHandler")>]
    member this.SetHandler (spec: CommandSpec<'Inputs, 'Output>, handler: 'Inputs -> 'Output) =
        newHandler handler spec

    [<Obsolete("'setCommand' has been deprecated in favor of 'addCommand' or 'addCommands`.")>]
    [<CustomOperation("setCommand")>] 
    member this.SetCommand (spec: CommandSpec<'Inputs, 'Output>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }

    [<CustomOperation("addCommand")>]
    member this.AddCommand (spec: CommandSpec<'Inputs, 'Output>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }

    [<CustomOperation("addCommands")>]
    member this.AddCommands (spec: CommandSpec<'Inputs, 'Output>, subCommands: System.CommandLine.Command seq) =
        { spec with SubCommands = spec.SubCommands @ (subCommands |> Seq.toList) }

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

        spec.SubCommands |> List.iter cmd.AddCommand
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

    [<CustomOperation("useTokenReplacer")>]
    member this.UseTokenReplacer(spec: CommandSpec<'Inputs, 'Output>, replacer: TryReplaceToken) =
        this.CommandLineBuilder.UseTokenReplacer(replacer) |> ignore
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
        
    [<CustomOperation("useTokenReplacer")>]
    member this.UseTokenReplacer(spec: CommandSpec<'Inputs, 'Output>, replacer: TryReplaceToken) =
        this.CommandLineBuilder.UseTokenReplacer(replacer) |> ignore
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
