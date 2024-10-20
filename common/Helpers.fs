module Helpers

open System
open System.IO

let formatDecimals (value: float) = value.ToString("0.00")

let getFileFromPath =
    let studentFile =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\StudentScores.txt")

    studentFile

let formatPathToRemoveSlash (value: string) : string = value.Replace("\\", "/")
