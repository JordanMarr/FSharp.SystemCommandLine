﻿module SubCommandAppTest

open System.IO
open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

let listCmd (handler: DirectoryInfo -> unit) = 
    let dir = Input.Argument("dir", DirectoryInfo(@"c:\fake dir"))

    command "list" {
        description "lists contents of a directory"
        inputs dir
        setHandler handler
    }

let deleteCmd (handler: DirectoryInfo * bool -> unit) = 
    let dir = Input.Argument("dir", DirectoryInfo(@"c:\fake dir"))
    let recursive = Input.Option("--recursive", false)

    command "delete" {
        description "deletes a directory"
        inputs (dir, recursive)
        setHandler handler
    }        

let rootCmd argstr listCmdHandler deleteCmdHandler =
    testRootCommand argstr  {
        description "File System Manager"
        setHandler id
        addCommand (listCmd listCmdHandler)
        addCommand (deleteCmd deleteCmdHandler)
    } 
    |> ignore

[<Test>]
let ``01 list c:\test`` () = 
    let mutable listCmdHandlerCalled = false

    rootCmd @"list ""c:\test""" 
        (fun (dir) -> 
            listCmdHandlerCalled <- true
            dir.FullName =! @"c:\test"
        )
        (fun (dir, recursive) -> 
            shouldNotCall ()
        )

    listCmdHandlerCalled =! true

[<Test>]
let ``02 delete c:\temp`` () =    
    let mutable deleteCmdHandlerCalled = false
    rootCmd @"delete ""c:\temp""" 
        (fun (dir) -> 
            shouldNotCall ()
        )
        (fun (dir, recursive) -> 
            deleteCmdHandlerCalled <- true
            dir.FullName =! @"c:\temp"
            recursive =! false
        )

    deleteCmdHandlerCalled =! true

[<Test>]
let ``03 delete c:\temp --recursive`` () =    
    let mutable deleteCmdHandlerCalled = false
    rootCmd @"delete ""c:\temp"" --recursive"
        (fun (dir) -> 
            shouldNotCall ()
        )
        (fun (dir, recursive) -> 
            deleteCmdHandlerCalled <- true
            dir.FullName =! @"c:\temp"
            recursive =! true
        )

    deleteCmdHandlerCalled =! true
