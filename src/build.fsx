#r "nuget: Fun.Build, 1.0.5"

open Fun.Build

let src = __SOURCE_DIRECTORY__

pipeline "CI Build" {

    stage "Build FSharp.SystemCommandLine.sln" {
        run $"dotnet restore {src}/FSharp.SystemCommandLine/FSharp.SystemCommandLine.fsproj"
        run $"dotnet build {src}/FSharp.SystemCommandLine/FSharp.SystemCommandLine.fsproj --configuration Release"
    }

    stage "Run Tests" {
        run $"dotnet test {src}/Tests/Tests.fsproj --configuration Release"
    }
    
    runIfOnlySpecified false
}

tryPrintPipelineCommandHelp ()