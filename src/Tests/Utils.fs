module Utils

open NUnit.Framework
open System.CommandLine
open System.CommandLine.Parsing
open FSharp.SystemCommandLine

/// Used to test the `rootCommand` with a command line arg string
let testRootCommand (commandLineString: string) = 
    let parser = Parsing.Parser()
    let result = parser.Parse(commandLineString)
    let args = result.Tokens |> Seq.map (fun t -> t.Value) |> Seq.toArray
    RootCommandBuilder args

let (|@) (test: bool) (failMsg: string) = 
    if not test then    
        Assert.Fail(failMsg)

