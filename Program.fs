module Program

open System.IO
open System

[<EntryPoint>]
let main argv =
    let studentFilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\StudentScores.txt")

    Student.summarize studentFilePath

    0
