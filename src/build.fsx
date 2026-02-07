#r "nuget: Fun.Build, 1.1.17"

open Fun.Build

let src = __SOURCE_DIRECTORY__
let fsproj = $"{src}/FSharp.SystemCommandLine/FSharp.SystemCommandLine.fsproj"

pipeline "Build" {

    stage "Build FSharp.SystemCommandLine.sln" {
        run $"dotnet restore {fsproj}"
        run $"dotnet build {fsproj} --configuration Release"
    }

    stage "Run Tests" {
        run $"dotnet test {src}/Tests/Tests.fsproj --configuration Release"
    }
    
    runIfOnlySpecified false
}

open System.Xml.Linq

pipeline "Publish" {

    stage "Pack" {
        run $"dotnet pack {fsproj} --configuration Release"
    }

    stage "Push to NuGet" {
        run (fun _ -> 
            let version = XDocument.Load(fsproj).Descendants(XName.Get "Version") |> Seq.head |> _.Value
            let nupkg = $"{src}/FSharp.SystemCommandLine/bin/Release/FSharp.SystemCommandLine.%s{version}.nupkg"
            let nugetKey = System.Environment.GetEnvironmentVariable("NUGET_KEY")
            $"dotnet nuget push %s{nupkg} --source https://api.nuget.org/v3/index.json --api-key %s{nugetKey} --skip-duplicate"
        )
    }

    runIfOnlySpecified true
}

tryPrintPipelineCommandHelp ()