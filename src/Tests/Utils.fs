module Utils

open System
open NUnit.Framework
open System.CommandLine
open System.CommandLine.Parsing
open System.CommandLine.Builder
open FSharp.SystemCommandLine

/// Used to test the `rootCommand` with a command line arg string
let testRootCommand (commandLineString: string) = 
    let parser = Parsing.Parser()
    let result = parser.Parse(commandLineString)
    let args = result.Tokens |> Seq.map (fun t -> t.Value) |> Seq.toArray
    RootCommandBuilder args

open FsUnit

let (|@) (test: bool) (failMsg: string) = 
    if not test then    
        Assert.Fail(failMsg)

let (@@) (test: bool) = 
    if not test then    
        Assert.Fail("Error: should be true")
