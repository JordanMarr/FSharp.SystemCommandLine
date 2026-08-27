#r "nuget: Partas.Build"
#r "nuget: Partas.TypeProvider.BuildHelper"

#nowarn 3886

open Partas.Build
open Partas.TypeProvider.BuildHelper

[<Literal>]
let root = __SOURCE_DIRECTORY__ + "\\.."

type Repo = BuildHelperProvider<root, "artifacts/", capabilityFullOverride = true>

// command line options
module Options =
    let quick =
        Input.option<bool> "--quick"
        |> Input.def false
        |> Input.desc "Skip restore/clean etc"
    let config =
        Baked.Input.DotNet.configString
        |> InputSpec.ofInput
        |> InputSpec.map (Option.defaultValue "Release")
    let skipTests =
        Input.option<bool> "--skip-tests"
        |> Input.def false
        |> Input.desc "Skip tests"
    // aliases for our projects
    let projectMap = Map.ofList [
        "fs-scl", Repo.Project.``FSharp.SystemCommandLine``.Path
    ]
    // useful if adding extra projects to the solution
    let target =
        Input.option<string list> "--project"
        |> Input.alias "-p"
        |> Input.desc "Target specific project(s)"
        |> Input.arity OneOrMore
        |> Input.allowMultipleArgumentsPerToken
        |> Input.def [ "fs-scl" ]
        |> Input.acceptOnlyFromAmong (projectMap |> Map.keys |> Seq.toList)
        |> InputSpec.ofInput
        |> InputSpec.map (List.map (fun project -> projectMap[project]))

// start defining common actions
let restore = input {
    let! quick = Options.quick
    return stage "restore" {
        quiet
        when' (not quick)
        run $"dotnet restore {Repo.FileSystem.src.``FSharp.SystemCommandLine.sln``.FullName}"
    }
}

let build = input {
    let! config = Options.config
    // could make this target projects if we wanted
    and! target = Options.target
    return stage "build" {
        quiet
        parallel'
        for project in target do
        stage $"build {project}" {
            run $"dotnet build {project} --configuration {config}"
        }
    }
}

let pack = input {
    let! config = Options.config
    and! target = Options.target
    return stage "pack" {
        quiet
        whenBranch "main"
        for project in target do
        stage $"pack {project}" {
            run $"dotnet pack {project} --configuration {config} --no-build --no-restore -o {Repo.VirtualFileSystem.artifacts.ToString()}"
        }
    }
}

let test = input {
    let! skipTests = Options.skipTests
    and! config = Options.config
    and! ci = Baked.Input.CI.isCI
    return stage "test" {
        quiet
        when' (not skipTests)
        outputTo (
            if ci
            then StageOutput.Silent
            else StageOutput.Console
            )
        run $"dotnet test {Repo.Project.Tests.Path} --configuration {config}"
    }
}

let push = input {
    let! apiKey = Baked.Input.NuGet.apiKeyOrEnv |> Input.required
    let pushPath = System.IO.Path.Combine(Repo.VirtualFileSystem.artifacts.ToString(), "*.nupkg")
    return stage "push" {
        when' apiKey.IsSome
        whenBranch "main"
        run $"dotnet nuget push {pushPath} --api-key {Cmd.secret apiKey.Value} --source https://api.nuget.org/v3/index.json --skip-duplicate"
    }
}

let bump = Baked.Pipelines.bumpArgument (Options.projectMap.Values |> Seq.toList) Options.target

rootCommand fsi.CommandLineArgs[1..] {
    description "Build script for FSharp.SystemCommandLine"
    command "bump" {
        description "Bump version numbers. Will fail if performed in CI. Do local, then commit, then push."
        bump
    }
    command "build" {
        description "Builds the solution"
        restore
        build
    }
    command "test" {
        description "Runs the tests"
        restore
        test
    }
    command "publish" {
        description "Publishes the packages"
        restore
        build
        test
        pack
        push
    }
}