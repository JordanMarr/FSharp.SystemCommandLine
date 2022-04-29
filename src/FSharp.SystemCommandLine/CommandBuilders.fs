[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Binding
open System.CommandLine.Builder
open System.CommandLine.Parsing

type private HI<'T> = HandlerInput<'T>
type private IC = System.CommandLine.Invocation.InvocationContext
let private def<'T> = Unchecked.defaultof<'T>

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
type BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>() = 

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

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>, l: HI<'L>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>, l: HI<'L>, m: HI<'M>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>, l: HI<'L>, m: HI<'M>, n: HI<'N>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>, l: HI<'L>, m: HI<'M>, n: HI<'N>, o: HI<'O>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N * 'O -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n; o ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: HI<'A>, b: HI<'B>, c: HI<'C>, d: HI<'D>, e: HI<'E>, f: HI<'F>, g: HI<'G>, h: HI<'H>, i: HI<'I>, j: HI<'J>, k: HI<'K>, l: HI<'L>, m: HI<'M>, n: HI<'N>, o: HI<'O>, p: HI<'P>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N * 'O * 'P -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n; o; p ] }

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
            | InjectedDependency -> () // DI system will inject this input
        )

        spec.SubCommands |> List.iter cmd.AddCommand
        cmd

    /// Sets a command handler that returns unit.
    member this.SetActionHandler (spec: CommandSpec<'Inputs, unit>) (cmd: Command) =
        let handler (args: obj) = spec.Handler (args :?> 'Inputs)

        let inputs = 
            spec.Inputs 
            |> List.choose (fun input -> 
                match input.Source with
                | ParsedOption o -> o :> IValueDescriptor |> Some
                | ParsedArgument a -> a :> IValueDescriptor |> Some
                | InjectedDependency -> None
            )
            |> List.toArray

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Action(fun () -> handler ()))
        | 01 -> cmd.SetHandler(Action<'A>(fun a -> handler (a)), inputs)
        | 02 -> cmd.SetHandler(Action<'A, 'B>(fun a b -> handler (a, b)), inputs)
        | 03 -> cmd.SetHandler(Action<'A, 'B, 'C>(fun a b c -> handler (a, b, c)), inputs)
        | 04 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D>(fun a b c d -> handler (a, b, c, d)), inputs)
        | 05 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E>(fun a b c d e -> handler (a, b, c, d, e)), inputs)
        | 06 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F>(fun a b c d e f -> handler (a, b, c, d, e, f)), inputs)
        | 07 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), inputs)
        | 08 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), inputs)
        | 09 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), inputs)
        | 10 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), inputs)
        | 11 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), inputs)
        | 12 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), inputs)
        | 13 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), inputs)
        | 14 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), inputs)
        | 15 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), inputs)
        | 16 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), inputs)
        | _ -> raise (NotImplementedException())
        cmd

    /// Sets a command handler that returns a status code.
    member this.SetFuncHandlerSync (spec: CommandSpec<'Inputs, int>) (cmd: Command) =
        let handler (args: obj) = 
            spec.Handler (args :?> 'Inputs)

        let inputs = 
            spec.Inputs 
            |> List.choose (fun input -> 
                match input.Source with
                | ParsedOption o -> o :> IValueDescriptor |> Some
                | ParsedArgument a -> a :> IValueDescriptor |> Some
                | InjectedDependency -> None
            )
            |> List.toArray

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Action<IC>(fun ctx -> ctx.ExitCode <- handler ()))
        | 01 -> cmd.SetHandler(Action<IC, 'A>(fun ctx a -> ctx.ExitCode <- handler (a)), inputs)
        | 02 -> cmd.SetHandler(Action<IC, 'A, 'B>(fun ctx a b -> ctx.ExitCode <- handler (a, b)), inputs)
        | 03 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C>(fun ctx a b c -> ctx.ExitCode <- handler (a, b, c)), inputs)
        | 04 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D>(fun ctx a b c d -> ctx.ExitCode <- handler (a, b, c, d)), inputs)
        | 05 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E>(fun ctx a b c d e -> ctx.ExitCode <- handler (a, b, c, d, e)), inputs)
        | 06 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F>(fun ctx a b c d e f -> ctx.ExitCode <- handler (a, b, c, d, e, f)), inputs)
        | 07 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G>(fun ctx a b c d e f g -> ctx.ExitCode <- handler (a, b, c, d, e, f, g)), inputs)
        | 08 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H>(fun ctx a b c d e f g h -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h)), inputs)
        | 09 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I>(fun ctx a b c d e f g h i -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i)), inputs)
        | 10 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J>(fun ctx a b c d e f g h i j -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j)), inputs)
        | 11 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K>(fun ctx a b c d e f g h i j k -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j, k)), inputs)
        | 12 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L>(fun ctx a b c d e f g h i j k l -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j, k, l)), inputs)
        | 13 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M>(fun ctx a b c d e f g h i j k l m -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), inputs)
        | 14 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N>(fun ctx a b c d e f g h i j k l m n -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), inputs)
        | 15 -> cmd.SetHandler(Action<IC, 'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O>(fun ctx a b c d e f g h i j k l m n o -> ctx.ExitCode <- handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), inputs)
        | _ -> raise (NotImplementedException())
        cmd

    /// Sets a command handler that returns a Task.
    member this.SetFuncHandlerAsync (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) (cmd: Command) =
        let handler (args: obj) = 
            task {
                return! spec.Handler (args :?> 'Inputs)
            }

        let inputs = 
            spec.Inputs 
            |> List.choose (fun input -> 
                match input.Source with
                | ParsedOption o -> o :> IValueDescriptor |> Some
                | ParsedArgument a -> a :> IValueDescriptor |> Some
                | InjectedDependency -> None
            )
            |> List.toArray

        match spec.Inputs.Length with
        | 00 -> cmd.SetHandler(Func<Task>(fun () -> handler ()))
        | 01 -> cmd.SetHandler(Func<'A, Task>(fun a -> handler (a)), inputs)
        | 02 -> cmd.SetHandler(Func<'A, 'B, Task>(fun a b -> handler (a, b)), inputs)
        | 03 -> cmd.SetHandler(Func<'A, 'B, 'C, Task>(fun a b c -> handler (a, b, c)), inputs)
        | 04 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, Task>(fun a b c d -> handler (a, b, c, d)), inputs)
        | 05 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, Task>(fun a b c d e -> handler (a, b, c, d, e)), inputs)
        | 06 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, Task>(fun a b c d e f -> handler (a, b, c, d, e, f)), inputs)
        | 07 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, Task>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), inputs)
        | 08 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, Task>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), inputs)
        | 09 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, Task>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), inputs)
        | 10 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, Task>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), inputs)
        | 11 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, Task>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), inputs)
        | 12 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, Task>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), inputs)
        | 13 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, Task>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), inputs)
        | 14 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, Task>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), inputs)
        | 15 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, Task>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), inputs)
        | 16 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, Task>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), inputs)
        | _ -> raise (NotImplementedException())
        cmd

            
/// Builds a `System.CommandLine.Parsing.Parser`.
type RootCommandParserBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>() = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()
    
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
        |> this.SetActionHandler spec
        |> ignore
        this.CommandLineBuilder.Build()

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerSync spec
        |> ignore
        this.CommandLineBuilder.Build()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerAsync spec
        |> ignore
        this.CommandLineBuilder.Build()

        
/// Builds and executes a `System.CommandLine.RootCommand`.
type RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(args: string array) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()
    
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
        |> this.SetActionHandler spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).Invoke()

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerSync spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).Invoke()

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        this.CommandLineBuilder.Command
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerAsync spec
        |> ignore
        this.CommandLineBuilder.Build().Parse(args).InvokeAsync()
       

/// Builds a `System.CommandLine.Command`.
type CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(name: string) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()
    
    /// Returns a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) = 
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetActionHandler spec

    /// Executes a Command with a handler that returns int.
    member this.Run (spec: CommandSpec<'Inputs, int>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerSync spec

    /// Executes a Command with a handler that returns a Task<unit> or Task<int>.
    member this.Run (spec: CommandSpec<'Inputs, Task<'ReturnValue>>) =
        Command(name)
        |> this.SetGeneralProperties spec
        |> this.SetFuncHandlerAsync spec


/// Builds a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommandParser<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output> = 
    RootCommandParserBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()

/// Builds and executes a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommand<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(args: string array)= 
    RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(args)

/// Builds a `System.CommandLine.Command` using computation expression syntax.
let command<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output> (name: string) = 
    CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(name)
