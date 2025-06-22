module Utils

open NUnit.Framework
open System.CommandLine
open System.CommandLine.Parsing
open FSharp.SystemCommandLine

/// Used to test the `rootCommand` with a command line arg string
let testRootCommand (commandLineString: string) =
    let cmd = Command("test")
    let result = CommandLineParser.Parse(cmd, commandLineString)
    let args = result.Tokens |> Seq.map (fun t -> t.Value) |> Seq.toArray
    RootCommandBuilder(args)

let shouldNotCall () = 
    Assert.Fail("should not call")

let (|@) (test: bool) (failMsg: string) = 
    if not test then    
        Assert.Fail(failMsg)

