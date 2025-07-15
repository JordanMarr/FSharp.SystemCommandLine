module Utils

open System.CommandLine.Parsing

let args commandLine =
    CommandLineParser.SplitCommandLine(commandLine) |> Seq.toArray
