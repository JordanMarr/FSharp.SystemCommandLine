module SubCommandAppTest

open NUnit.Framework
open FSharp.SystemCommandLine
open Utils
open FsUnit
open System.IO

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

[<Test>]
let Test1 () =    
    testRootCommand @"list ""c:\test"""  {
        description "File System Manager"
        setHandler id
        setCommand (listCmd (fun dir -> dir.FullName |> should equal @"c:\test"))
        setCommand (deleteCmd (fun (dir, recursive) -> shouldNotCall ()))
    } |> ignore

[<Test>]
let Test2 () =    
    testRootCommand @"delete ""c:\temp"""  {
        description "File System Manager"
        setHandler id
        setCommand (listCmd (fun dir -> shouldNotCall ()))
        setCommand (deleteCmd <|
            fun (dir, recursive) -> 
                dir.FullName |> should equal @"c:\temp"
                recursive |> should equal false
        )
    } |> ignore

[<Test>]
let Test3 () =    
    testRootCommand @"delete ""c:\temp"" --recursive"  {
        description "File System Manager"
        setHandler id
        setCommand (listCmd (fun dir -> shouldNotCall ()))
        setCommand (deleteCmd <|
            fun (dir, recursive) -> 
                dir.FullName |> should equal @"c:\temp"
                recursive |> should equal true
        )
    } |> ignore