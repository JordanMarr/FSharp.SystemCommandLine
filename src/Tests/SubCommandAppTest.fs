module SubCommandAppTest

open System.IO
open NUnit.Framework
open Utils
open FsUnit
open FSharp.SystemCommandLine
open System.CommandLine
open System.CommandLine.Builder

let listCmd (handler: DirectoryInfo -> unit) = 
    let dir = Input.Argument(getDefaultValue = (fun () -> DirectoryInfo("c:\fake dir")))

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setHandler handler
    }

let deleteCmd (handler: DirectoryInfo * bool -> unit) = 
    let dir = Input.Argument(getDefaultValue = (fun () -> DirectoryInfo("c:\fake dir")))    
    let recursive = Input.Option("--recursive", getDefaultValue = (fun () -> false))

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setHandler handler
    }        

let rootCmd argstr listCmdHandler deleteCmdHandler =
    testRootCommand argstr  {
        description "File System Manager"
        setHandler id
        //usePipeline (fun builder ->
        //    builder.UseExceptionHandler(fun ex ctx -> 
        //        failwith "YOU SHALL NOT PASS!!"
        //    )
        //)
        setCommand (listCmd listCmdHandler)
        setCommand (deleteCmd deleteCmdHandler)
    } 
    |> ignore

[<Test>]
let ``01 list c:\test`` () =    
    rootCmd @"list ""c:\test""" 
        (fun (dir) -> dir.FullName |> should equal @"c:\test")
        (fun (dir, recursive) -> shouldFail id)

[<Test>]
let ``02 delete c:\temp`` () =    
    rootCmd @"delete ""c:\temp""" 
        (fun (dir) ->
            shouldFail id)
        (fun (dir, recursive) -> 
                dir.FullName |> should equal @"c:\temp"
                recursive |> should equal false)

[<Test>]
let ``03 delete c:\temp --recursive`` () =    
    rootCmd @"delete ""c:\temp"" --recursive"
        (fun (dir) -> shouldFail id)
        (fun (dir, recursive) -> 
            dir.FullName |> should equal @"c:\temp"
            recursive |> should equal true)
