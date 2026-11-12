#r "nuget: Fun.Build, 1.1.17"

open Fun.Build
open System
open System.Diagnostics
open System.IO
open System.Runtime.InteropServices

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

// Use a unique package version so repeated local runs cannot reuse a cached build.
let smokeDir = Path.Combine(src, "TestNativeAot")
let smokePackages = Path.Combine(smokeDir, "obj", "packages")
let smokeOutput = Path.Combine(smokeDir, "bin", "native-aot")
let smokeVersion = "0.0.0-aot-" + Guid.NewGuid().ToString("N")
let smokeExecutable =
    Path.Combine(smokeOutput, if OperatingSystem.IsWindows() then "TestNativeAot.exe" else "TestNativeAot")

let checkNativeAot args expectedExitCode expectedOutput =
    let startInfo = ProcessStartInfo(smokeExecutable)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    for arg in args do startInfo.ArgumentList.Add(arg)
    use child = Process.Start(startInfo)
    let stdout = child.StandardOutput.ReadToEndAsync()
    let stderr = child.StandardError.ReadToEndAsync()
    if not (child.WaitForExit(30_000)) then
        child.Kill(true)
        failwithf "NativeAOT smoke test timed out: %A" args
    let output = stdout.GetAwaiter().GetResult()
    let errors = stderr.GetAwaiter().GetResult()
    if child.ExitCode <> expectedExitCode || not (output.Contains(expectedOutput: string)) then
        failwithf "NativeAOT smoke test %A: expected exit %d and output containing %A; got exit %d.\n%s\n%s"
            args expectedExitCode expectedOutput child.ExitCode output errors
    printfn "NativeAOT smoke test passed: %A" args

pipeline "NativeAOT" {
    // The consumer's global.json selects .NET 10 for both pack and publish.
    workingDir smokeDir

    stage "Pack library for NativeAOT" {
        run $"dotnet pack \"{fsproj}\" --configuration Release --output \"{smokePackages}\" -p:PackageVersion={smokeVersion}"
    }

    stage "Publish NativeAOT package consumer" {
        run $"dotnet publish TestNativeAot.fsproj --configuration Release --runtime {RuntimeInformation.RuntimeIdentifier} --output \"{smokeOutput}\" -p:SmokePackageVersion={smokeVersion}"
    }

    stage "Run NativeAOT smoke tests" {
        run (fun _ ->
            checkNativeAot [ "--help" ] 0 "--count"
            checkNativeAot [ "hello"; "world"; "--count"; "2" ] 0 "words=hello,world;count=2"
            checkNativeAot [] 0 "words=;count=0"
            checkNativeAot [ "--count"; "invalid" ] 1 ""
        )
    }

    runIfOnlySpecified true
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
