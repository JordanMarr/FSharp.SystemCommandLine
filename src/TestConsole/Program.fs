module Program

open System.CommandLine.Parsing

let args commandLine =
    CommandLineParser.SplitCommandLine(commandLine) |> Seq.toArray
    
[<EntryPoint>]
let main _ = 
    
    //ProgramAlt1.run ()
    //ProgramNoArgs.run ()
    ProgramNestedSubCommands.run () |> Async.AwaitTask |> Async.RunSynchronously // Also contains global options
    //ProgramSubCommand.run ()
    //ProgramTask.run ()
    //ProgramTokenReplacer.SCL.run ()
    //ProgramTokenReplacer.run ()
    //ProgramExtraInputs.run ()
    //ProgramAppendWords.run ()