module Utils

open NUnit.Framework
open System.CommandLine.Parsing
open FSharp.SystemCommandLine

let args commandLine =
    CommandLineParser.SplitCommandLine(commandLine) |> Seq.toArray

/// A small wrapper for testing the `rootCommand` CE that splits a single command line argument string into a string array.
let testRootCommand commandLine =
    RootCommandBuilder(args commandLine)

let shouldNotCall () = 
    Assert.Fail("should not call")

let (|@) (test: bool) (failMsg: string) = 
    if not test then Assert.Fail(failMsg)

