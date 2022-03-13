[<AutoOpen>]
module FSharp.SystemCommandLine.CommandBuilders

open System
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Binding

let private def<'T> = Unchecked.defaultof<'T>
exception MaxArgumentsExceeded

type CommandSpec<'Args, 'Result> = 
    {
        Description: string
        Options: System.CommandLine.Option list
        Handler: 'Args -> 'Result
        SubCommands: System.CommandLine.Command list
    }
    static member Default = 
        { 
            Description = "My Command"
            Options = []
            Handler = def<unit -> 'Result> // Support unit -> 'Result handler by default
            SubCommands = []
        }

/// Contains shared operations for building a `RootCommand` or `Command`.
type BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>() = 

    let newHandler handler spec =
        {
            Description = spec.Description
            Options = spec.Options
            Handler = handler
            SubCommands = spec.SubCommands
        }
    
    member this.Yield _ =
        // Set default of `unit * obj * obj` so that CE fails if no `options` are set
        CommandSpec<unit * obj * obj *obj, 'Result>.Default 

    // Prevents errors while typing join statement if rest of query is not filled in yet.
    member this.Zero _ = 
        CommandSpec<_, _>.Default

    [<CustomOperation("description")>]
    member this.Description (spec: CommandSpec<'T, 'U>, description) =
        { spec with Description = description }
    
    [<CustomOperation("options")>]
    member this.Options (spec: CommandSpec<'T, 'Result>, a: Opt<'A>) =
        { newHandler def<'A -> 'Result> spec with Options = [ a ] }
    
    [<CustomOperation("options")>]
    member this.Options (spec: CommandSpec<'T, 'Result>, (a: Opt<'A>, b: Opt<'B>)) =
        { newHandler def<'A * 'B -> 'Result> spec with Options = [ a; b ] }

    [<CustomOperation("options")>]
    member this.Options (spec: CommandSpec<'T, 'Result>, (a: Opt<'A>, b: Opt<'B>, c: Opt<'C>)) =
        { newHandler def<'A * 'B * 'C -> 'Result> spec with Options = [ a; b; c ] }

    [<CustomOperation("setHandler")>]
    member this.SetHandler (spec: CommandSpec<'Args, 'Return>, handler: 'Args -> 'Return) =
        newHandler handler spec

    [<CustomOperation("setCommand")>]
    member this.SetHandler (spec: CommandSpec<'Args, 'Return>, subCommand: System.CommandLine.Command) =
        { spec with SubCommands = spec.SubCommands @ [ subCommand ] }
               

/// Builds a `System.CommandLine.RootCommand`.
type RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>() = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>()
    
    let initRootCmd (spec: CommandSpec<'T, 'U>) = 
        let cmd = RootCommand()
        cmd.Description <- spec.Description
        spec.Options |> List.iter cmd.AddOption
        spec.SubCommands |> List.iter cmd.AddCommand
        cmd

    /// Executes a command that returns unit.
    member this.Run (spec: CommandSpec<'Args, unit>) =         
        let cmd = initRootCmd spec
        let opts = spec.Options |> Seq.cast<IValueDescriptor> |> Seq.toArray
        let handler (args: obj) = spec.Handler (args :?> 'Args)

        match spec.Options.Length with
        | 00 -> cmd.SetHandler(Action(fun () -> handler ()))
        | 01 -> cmd.SetHandler(Action<'A>(fun a -> handler (a)), opts)
        | 02 -> cmd.SetHandler(Action<'A, 'B>(fun a b -> handler (a, b)), opts)
        | 03 -> cmd.SetHandler(Action<'A, 'B, 'C>(fun a b c -> handler (a, b, c)), opts)
        | 04 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D>(fun a b c d -> handler (a, b, c, d)), opts)
        | 05 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E>(fun a b c d e -> handler (a, b, c, d, e)), opts)
        | 06 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F>(fun a b c d e f -> handler (a, b, c, d, e, f)), opts)
        | 07 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), opts)
        | 08 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), opts)
        | 09 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), opts)
        | 10 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), opts)
        | 11 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), opts)
        | 12 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), opts)
        | 13 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), opts)
        | 14 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), opts)
        | 15 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), opts)
        | 16 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), opts)
        | _ -> raise MaxArgumentsExceeded
        
        let args = Environment.GetCommandLineArgs()
        cmd.Invoke args

    /// Executes a command that returns a Task.
    member this.Run (spec: CommandSpec<'Args, Task<unit>>) =         
        let cmd = initRootCmd spec
        let opts = spec.Options |> Seq.cast<IValueDescriptor> |> Seq.toArray
        let handler (args: obj) = 
            task {
                do! spec.Handler (args :?> 'Args)
            }

        match spec.Options.Length with
        | 00 -> cmd.SetHandler(Func<Task>(fun () -> handler ()))
        | 01 -> cmd.SetHandler(Func<'A, Task>(fun a -> handler (a)), opts)
        | 02 -> cmd.SetHandler(Func<'A, 'B, Task>(fun a b -> handler (a, b)), opts)
        | 03 -> cmd.SetHandler(Func<'A, 'B, 'C, Task>(fun a b c -> handler (a, b, c)), opts)
        | 04 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, Task>(fun a b c d -> handler (a, b, c, d)), opts)
        | 05 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, Task>(fun a b c d e -> handler (a, b, c, d, e)), opts)
        | 06 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, Task>(fun a b c d e f -> handler (a, b, c, d, e, f)), opts)
        | 07 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, Task>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), opts)
        | 08 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, Task>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), opts)
        | 09 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, Task>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), opts)
        | 10 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, Task>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), opts)
        | 11 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, Task>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), opts)
        | 12 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, Task>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), opts)
        | 13 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, Task>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), opts)
        | 14 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, Task>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), opts)
        | 15 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, Task>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), opts)
        | 16 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, Task>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), opts)
        | _ -> raise MaxArgumentsExceeded
        
        let args = Environment.GetCommandLineArgs()
        cmd.InvokeAsync args
    

/// Builds a `System.CommandLine.Command`.
type CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>(name: string) = 
    inherit BaseCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>()
    
    let initRootCmd (spec: CommandSpec<'T, 'U>) = 
        let cmd = Command(name)
        cmd.Description <- spec.Description
        spec.Options |> List.iter cmd.AddOption
        spec.SubCommands |> List.iter cmd.AddCommand
        cmd

    /// Executes a command that returns unit.
    member this.Run (spec: CommandSpec<'Args, unit>) =         
       let cmd = initRootCmd spec
       let opts = spec.Options |> Seq.cast<IValueDescriptor> |> Seq.toArray
       let handler (args: obj) = spec.Handler (args :?> 'Args)

       match spec.Options.Length with
       | 00 -> cmd.SetHandler(Action(fun () -> handler ()))
       | 01 -> cmd.SetHandler(Action<'A>(fun a -> handler (a)), opts)
       | 02 -> cmd.SetHandler(Action<'A, 'B>(fun a b -> handler (a, b)), opts)
       | 03 -> cmd.SetHandler(Action<'A, 'B, 'C>(fun a b c -> handler (a, b, c)), opts)
       | 04 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D>(fun a b c d -> handler (a, b, c, d)), opts)
       | 05 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E>(fun a b c d e -> handler (a, b, c, d, e)), opts)
       | 06 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F>(fun a b c d e f -> handler (a, b, c, d, e, f)), opts)
       | 07 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), opts)
       | 08 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), opts)
       | 09 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), opts)
       | 10 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), opts)
       | 11 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), opts)
       | 12 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), opts)
       | 13 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), opts)
       | 14 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), opts)
       | 15 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), opts)
       | 16 -> cmd.SetHandler(Action<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), opts)
       | _ -> raise MaxArgumentsExceeded
       cmd

    /// Executes a command that returns a Task.
    member this.Run (spec: CommandSpec<'Args, Task<unit>>) =         
       let cmd = initRootCmd spec
       let opts = spec.Options |> Seq.cast<IValueDescriptor> |> Seq.toArray
       let handler (args: obj) = 
           task {
               do! spec.Handler (args :?> 'Args)
           }

       match spec.Options.Length with
       | 00 -> cmd.SetHandler(Func<Task>(fun () -> handler ()))
       | 01 -> cmd.SetHandler(Func<'A, Task>(fun a -> handler (a)), opts)
       | 02 -> cmd.SetHandler(Func<'A, 'B, Task>(fun a b -> handler (a, b)), opts)
       | 03 -> cmd.SetHandler(Func<'A, 'B, 'C, Task>(fun a b c -> handler (a, b, c)), opts)
       | 04 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, Task>(fun a b c d -> handler (a, b, c, d)), opts)
       | 05 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, Task>(fun a b c d e -> handler (a, b, c, d, e)), opts)
       | 06 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, Task>(fun a b c d e f -> handler (a, b, c, d, e, f)), opts)
       | 07 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, Task>(fun a b c d e f g -> handler (a, b, c, d, e, f, g)), opts)
       | 08 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, Task>(fun a b c d e f g h -> handler (a, b, c, d, e, f, g, h)), opts)
       | 09 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, Task>(fun a b c d e f g h i -> handler (a, b, c, d, e, f, g, h, i)), opts)
       | 10 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, Task>(fun a b c d e f g h i j -> handler (a, b, c, d, e, f, g, h, i, j)), opts)
       | 11 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, Task>(fun a b c d e f g h i j k -> handler (a, b, c, d, e, f, g, h, i, j, k)), opts)
       | 12 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, Task>(fun a b c d e f g h i j k l -> handler (a, b, c, d, e, f, g, h, i, j, k, l)), opts)
       | 13 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, Task>(fun a b c d e f g h i j k l m -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m)), opts)
       | 14 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, Task>(fun a b c d e f g h i j k l m n -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n)), opts)
       | 15 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, Task>(fun a b c d e f g h i j k l m n o -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o)), opts)
       | 16 -> cmd.SetHandler(Func<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, Task>(fun a b c d e f g h i j k l m n o p -> handler (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p)), opts)
       | _ -> raise MaxArgumentsExceeded
       cmd


/// Builds a `System.CommandLine.RootCommand` using computation expression syntax.
let rootCommand<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result> = 
    RootCommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>()

/// Builds a `System.CommandLine.Command` using computation expression syntax.
let command<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result> (name: string) = 
    CommandBuilder<'A, 'B, 'C, 'D, 'E, 'F, 'G, 'H, 'I, 'J, 'K, 'L, 'M, 'N, 'O, 'P, 'Result>(name)
