module ProgramNoArgs

open FSharp.SystemCommandLine
    
let app () = 
    printfn "Do stuff"

//[<EntryPoint>]
let main argv = 
    rootCommand argv {
        description "My sample app"
        setAction app
    }
    
        
