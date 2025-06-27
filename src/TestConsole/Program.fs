module Program

open System.CommandLine.Parsing

let args commandLine =
    CommandLineParser.SplitCommandLine(commandLine) |> Seq.toArray
    
[<EntryPoint>]
let main _ = 
    
    //ProgramAlt1.main (args "--int-option 1 --bool-option true --file-option \"c:\test\"")
    
    //ProgramNoArgs.main (args "")
    
    //ProgramNestedSubCommands.main (args "io list \"c:/data/\" --enable-logging") // Also contains global options
    
    //ProgramSubCommand.main (args "list c:/data/")
    //ProgramSubCommand.main (args "delete c:/data/ --recursive")
    
    //ProgramTask.main (args "-w hello -w world")
    
    //ProgramTokenReplacer.main (args "--package @shoelace-style/shoelace")
    
    //ProgramExtraInputs.main (args "-a A -b B -c C -d D -e E -1 1 -2 2 -3 3 -4 4 -5 5")
    
    ProgramAppendWords.main (args "-w hello -w world -s \", \"")
