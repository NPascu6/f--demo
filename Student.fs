module Student

open Logger
open System.IO
open System

type Grade = { Value: float; Name: string }

type Student =
    { Name: string
      Id: string
      AverageGrade: float
      HighestGrade: Grade
      LowestGrade: Grade }

let fromString (row: string, head: string[]) =
    try
        let columns = row.Split('\t')
        let scores = columns |> Array.skip 2 |> Array.map float
        let highestGrade = scores |> Array.maxBy float
        let highestGradeString = highestGrade.ToString("F2")
        let lowestGrade = scores |> Array.minBy float
        let lowestGradeString = lowestGrade.ToString("F2")

        //get the index of the highest and lowest grade if format is wrong raise exception
        try
            let indexOfHighestGrade =
                columns |> Array.findIndex (fun x -> x = highestGradeString)

            let indexOfLowestGrade = columns |> Array.findIndex (fun x -> x = lowestGradeString)

            { Name = columns.[0]
              Id = columns.[1]
              AverageGrade = scores |> Array.average
              HighestGrade =
                { Value = highestGrade
                  Name = head.[indexOfHighestGrade] }
              LowestGrade =
                { Value = lowestGrade
                  Name = head.[indexOfLowestGrade] } }
        with :? FormatException as ex ->
            raise (new FormatException("Invalid row format. Please check the data."))
    with :? FormatException as ex ->
        raise (new FormatException($"Invalid row format. Please check the data. Details:{ex.Message}"))

let logSummary student (logger: ILogger) =
    logger.Log($"Student: {student.Name}")
    logger.Log($"Average Grade: {student.AverageGrade}")
    logger.Log($"Highest Grade: {student.HighestGrade.Name}: {student.HighestGrade.Value}")
    logger.Log($"Lowest Grade: {student.LowestGrade.Name}: {student.LowestGrade.Value}")
    logger.Log("----------------------------------------\n")

let printSummary student =
    printfn ($"Student: {student.Name}")
    printfn ($"Average Grade: {student.AverageGrade}")
    printfn ($"Highest Grade: {student.HighestGrade.Name}: {student.HighestGrade.Value}")
    printfn ($"Lowest Grade: {student.LowestGrade.Name}: {student.LowestGrade.Value}")
    printfn ("----------------------------------------\n")

let print student method =
    let logger = FileLogger() :> ILogger

    match method with
    | "logger" -> logSummary student logger
    | "console" -> printSummary student
    | _ -> failwith "Invalid method. Please use 'logger' or 'console'."


let summarize filePath =
    let logger = FileLogger() :> ILogger

    try
        if File.Exists filePath then
            logger.Log $"Reading file from: {filePath}"
            let data = File.ReadAllLines(filePath)

            if data.Length > 0 then
                // Read all lines from the file
                let rows = data |> Array.skip 1
                let head = rows |> Array.head |> (fun x -> x.Split('\t'))

                rows
                |> Array.map (fun row -> fromString (row, head))
                |> Array.sortByDescending (fun student -> student.AverageGrade)
                |> Array.iter (fun student -> print student "logger")

                logger.Log "All actions completed successfully."
            else
                logger.Log "The file is empty. No data to process."

        else
            logger.Log $"File {filePath} does not exist. Please check the path."
    with ex ->
        // Log any exception that occurs
        logger.LogError $"An error occurred: {ex.Message}"
