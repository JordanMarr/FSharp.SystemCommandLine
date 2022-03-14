[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Binding
open System.CommandLine.Builder
open System.CommandLine.Parsing

type private IVD<'T> = IValueDescriptor<'T>
let private def<'T> = Unchecked.defaultof<'T>

type CommandSpec<'Inputs, 'Output> = 
    {
        Description: string
        Inputs: IValueDescriptor list
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
    member this.Inputs (spec: CommandSpec<'T, 'Output>, a: IVD<'A>) =
        { newHandler def<'A -> 'Output> spec with Inputs = [ a ] }
    
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>)) =
        { newHandler def<'A * 'B -> 'Output> spec with Inputs = [ a; b ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>)) =
        { newHandler def<'A * 'B * 'C -> 'Output> spec with Inputs = [ a; b; c ] }
        
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>)) =
        { newHandler def<'A * 'B * 'C * 'D -> 'Output> spec with Inputs = [ a; b; c; d ] }
            
    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E -> 'Output> spec with Inputs = [ a; b; c; d; e ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F -> 'Output> spec with Inputs = [ a; b; c; d; e; f ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>, l: IVD<'L>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>, l: IVD<'L>, m: IVD<'M>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>, l: IVD<'L>, m: IVD<'M>, n: IVD<'N>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>, l: IVD<'L>, m: IVD<'M>, n: IVD<'N>, o: IVD<'O>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N * 'O -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n; o ] }

    [<CustomOperation("inputs")>]
    member this.Inputs (spec: CommandSpec<'T, 'Output>, (a: IVD<'A>, b: IVD<'B>, c: IVD<'C>, d: IVD<'D>, e: IVD<'E>, f: IVD<'F>, g: IVD<'G>, h: IVD<'H>, i: IVD<'I>, j: IVD<'J>, k: IVD<'K>, l: IVD<'L>, m: IVD<'M>, n: IVD<'N>, o: IVD<'O>, p: IVD<'P>)) =
        { newHandler def<'A * 'B * 'C * 'D * 'E * 'F * 'G * 'H * 'I * 'J * 'K * 'L * 'M * 'N * 'O * 'P -> 'Output> spec with Inputs = [ a; b; c; d; e; f; g; h; i; j; k; l; m; n; o; p ] }

    [<CustomOperation("setHandler")>]
    member this.SetHandler (spec: CommandSpec<'Inputs, 'Output>, handler: 'Inputs -> 'Output) =
        newHandler handler spec

    [<CustomOperation("setCommand")>]
    member this.SetHandler (spec: CommandSpec<'Inputs, 'Output>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }

    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> unit) =
        subCommand this.CommandLineBuilder
        spec

    [<CustomOperation("usePipeline")>]
    member this.UsePipeline (spec: CommandSpec<'Inputs, 'Output>, subCommand: CommandLineBuilder -> CommandLineBuilder) =
        this.CommandLineBuilder <- subCommand this.CommandLineBuilder
        spec

    /// Executes a command that returns unit.
    member this.CreateActionCommand (spec: CommandSpec<'Inputs, unit>, initCmd: CommandSpec<_, _> -> Command) =
       let cmd = initCmd spec
       let inputs = spec.Inputs |> List.toArray
       let handler (args: obj) = spec.Handler (args :?> 'Inputs)

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

    /// Executes a command that returns a Task.
    member this.CreateFuncCommand (spec: CommandSpec<'Inputs, Task<unit>>, initCmd: CommandSpec<_, _> -> Command) =         
       let cmd = initCmd spec
       let inputs = spec.Inputs |> List.toArray
       let handler (args: obj) = 
           task {
               do! spec.Handler (args :?> 'Inputs)
           }

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
            
            
/// Builds a `System.CommandLine.RootCommand`.
type RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(args: string array) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()
    
    /// Applies the spec to the pipeline-created command.
    let applySpecToRootCommand (builder: CommandLineBuilder) (spec: CommandSpec<'T, 'U>) = 
        let cmd = builder.Command
        cmd.Description <- spec.Description
        spec.Inputs |> List.choose (function | :? Option as opt -> Some opt | _ -> None) |> List.iter cmd.AddOption
        spec.Inputs |> List.choose (function | :? Argument as arg -> Some arg | _ -> None) |> List.iter cmd.AddArgument
        spec.SubCommands |> List.iter cmd.AddCommand
        cmd

    /// Executes a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =         
        this.CreateActionCommand(spec, applySpecToRootCommand this.CommandLineBuilder) |> ignore
        this.CommandLineBuilder.Build().Invoke(args)

    /// Executes a Command with a handler that returns a Task.
    member this.Run (spec: CommandSpec<'Inputs, Task<unit>>) =         
        this.CreateFuncCommand(spec, applySpecToRootCommand this.CommandLineBuilder) |> ignore
        this.CommandLineBuilder.Build().InvokeAsync(args)
    

/// Builds a `System.CommandLine.Command`.
type CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(name: string) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>()
    
    /// Creates a command and applies the spec.
    let applySpecToCommand (spec: CommandSpec<'T, 'U>) = 
        let cmd = Command(name)
        cmd.Description <- spec.Description
        spec.Inputs |> List.choose (function | :? Option as opt -> Some opt | _ -> None) |> List.iter cmd.AddOption
        spec.Inputs |> List.choose (function | :? Argument as arg -> Some arg | _ -> None) |> List.iter cmd.AddArgument
        spec.SubCommands |> List.iter cmd.AddCommand
        cmd

    /// Returns a Command with a handler that returns unit.
    member this.Run (spec: CommandSpec<'Inputs, unit>) =         
        this.CreateActionCommand(spec, applySpecToCommand)

    /// Returns a Command with a handler that returns a Task.
    member this.Run (spec: CommandSpec<'Inputs, Task<unit>>) =         
        this.CreateFuncCommand(spec, applySpecToCommand)


/// Builds a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommand<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output> = 
    let args = Environment.GetCommandLineArgs()
    RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(args)

/// Builds a `System.CommandLine.Command` using computation expression syntax.
let command<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output> (name: string) = 
    CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Output>(name)
