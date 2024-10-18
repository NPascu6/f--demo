module Program

open System
open System.IO
open Logger
open Actions

[<EntryPoint>]
let main argv =
    // Initialize logger
    let logger = FileLogger() :> ILogger

    // Get the file path in a cross-platform way
    let filePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files", "StudentScores.txt")

    try
        if File.Exists filePath then
            logger.Log $"Reading file from: {filePath}"

            // Read all lines from the file
            let rows = File.ReadAllLines(filePath)

            if rows.Length > 0 then
                let firstRow = rows.[0]

                // Log summarizing info and student scores
                summarize (logger, rows)
                printScorePerStudent (logger, rows, firstRow)

                // Prepare async tasks
                let tasks =
                    [ getStudentsThatPassed (logger, rows)
                      getStudentsThatFailed (logger, rows)
                      getStudentsWithScholarship (logger, rows)
                      averageGradePerClass (logger, rows)
                      averageGradePerPassing (logger, rows)
                      averageGradePerFailing (logger, rows)
                      averageGradePerScholarship (logger, rows)
                      highestPassedGrade (logger, rows)
                      lowestPassedGrade (logger, rows)
                      highestFailedGrade (logger, rows)
                      lowestFailedGrade (logger, rows) ]

                // Run all tasks in parallel
                tasks |> Async.Parallel |> Async.RunSynchronously |> ignore

                logger.Log "All actions completed successfully."
            else
                logger.Log "The file is empty. No data to process."

        else
            logger.Log $"File {filePath} does not exist. Please check the path."
    with ex ->
        // Log any exception that occurs
        logger.LogError $"An error occurred: {ex.Message}"

    0
