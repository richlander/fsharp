﻿module FSharp.Compiler.Service.Tests.CompletionTests

open FSharp.Compiler.EditorServices
open NUnit.Framework

let getCompletionInfo lineText (line, column) source =
    let parseResults, checkResults = getParseAndCheckResultsPreview source
    let plid = QuickParse.GetPartialLongNameEx(lineText, column)
    checkResults.GetDeclarationListInfo(Some parseResults, line, lineText, plid)

let getCompletionItemNames (completionInfo: DeclarationListInfo) =
    completionInfo.Items |> Array.map (fun item -> item.Name)

let assertHasItemWithNames names (completionInfo: DeclarationListInfo) =
    let itemNames = getCompletionItemNames completionInfo |> set

    for name in names do
        Assert.That(Set.contains name itemNames, $"{name} not found in {itemNames}")

[<Test>]
let ``Expr - After record decl`` () =
    let info = getCompletionInfo "{ Fi }" (4, 0)  """
type Record = { Field: int }


"""
    assertHasItemWithNames ["ignore"] info

[<Test>]
let ``Expr - record - field 01 - anon module`` () =
    let info = getCompletionInfo "{ Fi }" (4, 3)  """
type Record = { Field: int }

{ Fi }
"""
    assertHasItemWithNames ["Field"] info

[<Test>]
let ``Expr - record - field 02 - anon module`` () =
    let info = getCompletionInfo "{ Fi }" (6, 3)  """
type Record = { Field: int }

let record = { Field = 1 }

{ Fi }
"""
    assertHasItemWithNames ["Field"] info

[<Test>]
let ``Expr - record - empty 01`` () =
    let info = getCompletionInfo "{  }" (4, 2) """
type Record = { Field: int }

{  }
"""
    assertHasItemWithNames ["Field"] info

[<Test>]
let ``Expr - record - empty 02`` () =
    let info = getCompletionInfo "{  }" (6, 2) """
type Record = { Field: int }

let record = { Field = 1 }

{  }
"""
    assertHasItemWithNames ["Field"; "record"] info

[<Test>]
let ``Underscore dot lambda - completion`` () =
    let info = getCompletionInfo "    |> _.Len" (4, 11) """
let myFancyFunc (x:string) = 
    x 
    |> _.Len"""
    assertHasItemWithNames ["Length"] info

[<Test>]
let ``Underscore dot lambda - method completion`` () =
    let info = getCompletionInfo "    |> _.ToL" (4, 11) """
let myFancyFunc (x:string) = 
    x 
    |> _.ToL"""
    assertHasItemWithNames ["ToLower"] info

[<Test>]
let ``Type decl - Record - Field type 01`` () =
    let info = getCompletionInfo "type Record = { Field:  }" (2, 23)  """
type Record = { Field:  }
"""
    assertHasItemWithNames ["string"] info
