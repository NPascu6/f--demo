module Student

open Logger
open System.IO
open System
open StudentModel

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
            raise (new FormatException($"Invalid row format. Please check the data. Details: {ex.Message}"))
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


let summarize filePath (commandArg: string[]) =
    let logger = FileLogger() :> ILogger
    //let the value of commandArg be "logger" if it is empty
    let mutable commandArgVar = commandArg

    if
        (commandArg.Length > 0
         && (commandArg = [| "logger" |] || commandArg = [| "console" |]))
    then
        logger.Log($"Command line arguments: {commandArg}")

        if (commandArg.[0] = "console") then
            logger.Log("Printing to console.")
        else if (commandArg.[0] = "logger") then
            logger.Log("Printing to logger.")
        else
            logger.LogError("Invalid command line argument. Please use 'console' or 'logger'.")
    else
        logger.LogError("No command line arguments provided. Please provide 'console' or 'logger'.")


    try
        if File.Exists filePath then
            logger.Log $"Reading file from: {filePath}"
            let data = File.ReadAllLines(filePath)

            if data.Length > 0 then
                // Read all lines from the file
                let rows = data |> Array.skip 1
                let head = data |> Array.head |> (fun x -> x.Split('\t'))
                let printMethod = commandArg.[0]

                rows
                |> Array.map (fun row -> fromString (row, head))
                |> Array.sortByDescending (fun student -> student.AverageGrade)
                |> Array.iter (fun student -> print student commandArgVar[0])

                logger.Log "All actions completed successfully."
                logger.Log $"Log file path: {Helpers.formatPathToRemoveSlash logger.defaultPath}"
            else
                logger.Log "The file is empty. No data to process."

        else
            logger.Log $"File {filePath} does not exist. Please check the path."
    with ex ->
        // Log any exception that occurs
        logger.LogError $"An error occurred: {ex.Message}"
