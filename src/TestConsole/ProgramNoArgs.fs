module ProgramNoArgs

open FSharp.SystemCommandLine
    
let app () = 
    printfn "Do stuff"

//[<EntryPoint>]
let main argv = 
    rootCommand {
        description "My sample app"
        setHandler app
    }
    
        
