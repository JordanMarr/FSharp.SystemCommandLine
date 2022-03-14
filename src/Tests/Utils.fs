module Utils

open NUnit.Framework
open System.CommandLine
open FSharp.SystemCommandLine
open System

/// Used to test the `rootCommand` with manual command line args
let testRootCommand args = RootCommandBuilder args

/// Splits string into a string array
let splitBySpace (s: string) = s.Split(' ')
