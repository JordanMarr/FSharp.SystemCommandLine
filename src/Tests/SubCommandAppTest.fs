module SubCommandAppTest

open System.IO
open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine
open Utils

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
        //    CommandLineBuilder() // Remove `UseDefaults`
        //)
        setCommand (listCmd listCmdHandler)
        setCommand (deleteCmd deleteCmdHandler)
    } 
    |> ignore

[<Test>]
let ``01 list c:\test`` () =    
    rootCmd @"list ""c:\test""" 
        (fun (dir) -> dir.FullName =! @"c:\test")
        (fun (dir, recursive) -> Assert.Fail())

[<Test>]
let ``02 delete c:\temp`` () =    
    rootCmd @"delete ""c:\temp""" 
        (fun (dir) -> Assert.Fail("should not call"))
        (fun (dir, recursive) -> 
                dir.FullName =! @"c:\temp"
                recursive =! false)

[<Test>]
let ``03 delete c:\temp --recursive`` () =    
    rootCmd @"delete ""c:\temp"" --recursive"
        (fun (dir) -> Assert.Fail("should not call"))
        (fun (dir, recursive) -> 
            dir.FullName = @"c:\temp" |@ "bad delete path"
            recursive =! true)
