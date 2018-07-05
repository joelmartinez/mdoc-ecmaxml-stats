// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Xml.Linq

type Data = {MemberCount: int; TypeCount:int; MemberBorked:int;TypeBorked:int;MemberTooLong:int;TypeTooLong:int;MemberNoType:int;TypeNoType:int;}

[<EntryPoint>]
let main argv =
    let androidPath = "/Users/jmartinez/dev/xamarin/android-api-docs/docs/Mono.Android/en"

    let xmls = Directory.GetFiles(androidPath, "*.xml", SearchOption.AllDirectories);

    let allxml = xmls 
                 |> Seq.map File.ReadAllText 
                 |> Seq.filter (fun x -> x.StartsWith("<Type Name="))
    let xdocs = allxml |> Seq.map XDocument.Parse

    let xn s = XName.Get(s)
    let value (x:XElement) = 
        let reader = x.CreateReader()
        reader.MoveToContent() |> ignore
        reader.ReadInnerXml()

    let typesummaries = xdocs 
                        |> Seq.collect (fun x -> x.Root
                                                  .Elements(xn "Docs")
                                                  .Elements(xn "summary"))
                        |> Seq.map value
                        |> Seq.toArray
    let membersummaries = xdocs 
                          |> Seq.collect (fun x -> x.Root
                                                    .Elements(xn "Members")
                                                    .Elements(xn "Member")
                                                    .Elements(xn "Docs")
                                                    .Elements(xn "summary"))
                          |> Seq.map value
                          |> Seq.toArray

    let PROBABLY_A_MISTAKE = 500
    let noType (x:string) = x.Contains("NoType")
    let tooLong (x:string) = x.Length > PROBABLY_A_MISTAKE
    let allBad (x:string) = noType x && tooLong x 
    let data = { 
                      MemberCount = (membersummaries |> Seq.length); 
                      TypeCount = (typesummaries |> Seq.length);
                      MemberBorked = (membersummaries 
                                      |> Seq.filter allBad 
                                      |> Seq.length);
                      TypeBorked = (typesummaries 
                                      |> Seq.filter allBad 
                                      |> Seq.length)
                      MemberNoType = (membersummaries 
                                      |> Seq.filter noType 
                                      |> Seq.length);
                      TypeNoType = (typesummaries 
                                      |> Seq.filter noType 
                                      |> Seq.length);
                      TypeTooLong = (typesummaries 
                                      |> Seq.filter tooLong 
                                      |> Seq.length);
                      MemberTooLong = (membersummaries 
                                      |> Seq.filter tooLong 
                                      |> Seq.length);
                                      }

    printfn "'too long length': %i" PROBABLY_A_MISTAKE
    printf "type\n\tcount: %i\n\ttoo long: %i\n\tNoType: %i\n\tboth:%i\nmember\n\tcount: %i\n\ttoo long: %i\n\tNoType: %i\n\tboth:%i" data.TypeCount data.TypeTooLong data.TypeNoType data.TypeBorked data.MemberCount data.MemberTooLong data.MemberNoType data.MemberBorked

    printf "\n\nType Summary that's too long:\n%s" (typesummaries |> Seq.filter tooLong |> Seq.head)
    printf "\n-------\nMember Summary that's too long:\n%s" (membersummaries |> Seq.filter tooLong |> Seq.head)
    0 // return an integer exit code
