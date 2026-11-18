module TrimmingDescriptorTest

open System.Reflection
open System.Xml.Linq
open NUnit.Framework
open Swensen.Unquote
open FSharp.SystemCommandLine

/// The library embeds an ILLink descriptor so trimmed/NativeAOT apps keep members it looks up by name.
/// This verifies the descriptor is embedded and that every type/method it names still exists.
[<Test>]
let ``Trimming descriptor is embedded and every entry resolves`` () =
    let lib = typeof<ActionInput>.Assembly
    use stream = lib.GetManifestResourceStream "ILLink.Descriptors.xml"
    test <@ not (isNull stream) @>

    let xml = XDocument.Load stream
    let assemblies = xml.Root.Elements(XName.Get "assembly") |> Seq.toList
    test <@ not assemblies.IsEmpty @>

    for asm in assemblies do
        let target = Assembly.Load(asm.Attribute(XName.Get "fullname").Value)
        for typ in asm.Elements(XName.Get "type") do
            let typeName = typ.Attribute(XName.Get "fullname").Value
            let t = target.GetType typeName
            test <@ not (isNull t) @>
            for m in typ.Elements(XName.Get "method") do
                let name = m.Attribute(XName.Get "name").Value
                test <@ t.GetMethods() |> Array.exists (fun mi -> mi.Name = name) @>
