module ProgramNoArgs

open FSharp.SystemCommandLine
    
let app () = 
    printfn "Do stuff"

let main argv = 
    rootCommand argv {
        description "My sample app"
        setAction app
    }
    
        
