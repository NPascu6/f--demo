module Actions

open Logger
open System

// Helper function to log messages to both logger and console
let logAndPrint (logger: ILogger) (msg: string) =
    logger.Log msg
    Console.WriteLine msg

// Common function to compute average or handle collections
let computeAverage (values: string[]) startIndex =
    values |> Array.skip startIndex |> Array.averageBy float

// Common function to filter students based on a condition
let filterStudents (rows: string[]) condition =
    rows |> Array.skip 1 |> Array.filter condition

// Summarize function
let summarize (logger: ILogger, rows: string[]) =
    if (rows = null) then
        logAndPrint logger "Data is null"
    elif (rows.Length = 0) then
        logAndPrint logger "Data is empty"
    else
        let studentCount = (rows |> Array.length) - 1
        logAndPrint logger $"File has {rows.Length} lines"
        logAndPrint logger $"File has {studentCount} students"

// Get highest grade column name
let getHighestGradeColumnName (logger: ILogger, row: string, head: string) =
    let columns = row.Split('\t')
    let highestGrade = columns |> Array.skip 2 |> Array.maxBy float

    let highestGradeIndex =
        columns |> Array.findIndex (fun x -> x = highestGrade.ToString())

    let highestGradeColumnName = head.Split('\t').[highestGradeIndex]
    logAndPrint logger $"Highest grade: {highestGradeColumnName} with {Helpers.formatDecimals (float highestGrade)}"

// Print main score for a student
let printMainScore (logger: ILogger, row: string, head: string, index) =
    async {
        let columns = row.Split('\t')

        if columns.Length > 2 then
            let averageGrades = computeAverage columns 2
            logAndPrint logger $"Student number: {index} and Name: {columns.[0]}"
            logAndPrint logger $"Average score: {Helpers.formatDecimals averageGrades}"
            getHighestGradeColumnName (logger, row, head)
            Console.WriteLine("-----------------------------------\n")
    }

// Print scores for each student
let printScorePerStudent (logger: ILogger, rows: string[], head: string) =
    rows
    |> Array.skip 1
    |> Array.iteri (fun i row -> printMainScore (logger, row, head, i + 1) |> Async.RunSynchronously)

// Get students that passed
let getStudentsThatPassed (logger: ILogger, rows: string[]) =
    async {
        if (rows.Length = 0) then
            logAndPrint logger "No students found"
        elif (rows = null) then
            logAndPrint logger "Data is null"
        else
            let passedStudents =
                filterStudents rows (fun x -> x.Split('\t').[2] |> float > 50.0)

            logAndPrint logger $"Number of students that passed: {passedStudents.Length}"
    }

// Get students that failed
let getStudentsThatFailed (logger: ILogger, rows: string[]) =
    async {
        if (rows.Length = 0) then
            logAndPrint logger "No students found"
        elif (rows = null) then
            logAndPrint logger "Data is null"
        else
            let failedStudents =
                filterStudents rows (fun x -> x.Split('\t').[2] |> float < 50.0)

            logAndPrint logger $"Number of students that failed: {failedStudents.Length}"
    }

// Get students with scholarship
let getStudentsWithScholarship (logger: ILogger, rows: string[]) =
    async {
        let studentsWithScholarship =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float > 70.0)

        logAndPrint logger $"Number of students with scholarship: {studentsWithScholarship.Length}"
    }

// Calculate average grade for a class
let averageGradePerClass (logger: ILogger, rows: string[]) =
    async {
        let students = rows |> Array.skip 1

        let averageGrade =
            students |> Array.map (fun x -> x.Split('\t').[2] |> float) |> Array.average

        logAndPrint logger $"Average grade per class: {Helpers.formatDecimals averageGrade}"
    }

// Calculate average grade for passing students
let averageGradePerPassing (logger: ILogger, rows: string[]) =
    async {
        let passingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float > 50.0)

        let averageGrade =
            passingStudents
            |> Array.map (fun x -> x.Split('\t').[2] |> float)
            |> Array.average

        logAndPrint logger $"Average grade per passing student: {Helpers.formatDecimals averageGrade}"
    }

// Calculate average grade for failing students
let averageGradePerFailing (logger: ILogger, rows: string[]) =
    async {
        let failingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float < 50.0)

        let averageGrade =
            failingStudents
            |> Array.map (fun x -> x.Split('\t').[2] |> float)
            |> Array.average

        logAndPrint logger $"Average grade per failing student: {Helpers.formatDecimals averageGrade}"
    }

// Calculate average grade for students with scholarship
let averageGradePerScholarship (logger: ILogger, rows: string[]) =
    async {
        let studentsWithScholarship =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float > 70.0)

        let averageGrade =
            studentsWithScholarship
            |> Array.map (fun x -> x.Split('\t').[2] |> float)
            |> Array.average

        logAndPrint logger $"Average grade per student with scholarship: {Helpers.formatDecimals averageGrade}"
    }

// Get highest passed grade
let highestPassedGrade (logger: ILogger, rows: string[]) =
    async {
        let passingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float > 50.0)

        let highestGrade =
            passingStudents |> Array.map (fun x -> x.Split('\t').[2] |> float) |> Array.max

        logAndPrint logger $"Highest grade of passing student: {Helpers.formatDecimals highestGrade}"
    }

// Get lowest passed grade
let lowestPassedGrade (logger: ILogger, rows: string[]) =
    async {
        let passingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float > 50.0)

        let lowestGrade =
            passingStudents |> Array.map (fun x -> x.Split('\t').[2] |> float) |> Array.min

        logAndPrint logger $"Lowest grade of passing student: {Helpers.formatDecimals lowestGrade}"
    }

// Get highest failed grade
let highestFailedGrade (logger: ILogger, rows: string[]) =
    async {
        let failingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float < 50.0)

        let highestGrade =
            failingStudents |> Array.map (fun x -> x.Split('\t').[2] |> float) |> Array.max

        logAndPrint logger $"Highest grade of failing student: {Helpers.formatDecimals highestGrade}"
    }

// Get lowest failed grade
let lowestFailedGrade (logger: ILogger, rows: string[]) =
    async {
        let failingStudents =
            filterStudents rows (fun x -> x.Split('\t').[2] |> float < 50.0)

        let lowestGrade =
            failingStudents |> Array.map (fun x -> x.Split('\t').[2] |> float) |> Array.min

        logAndPrint logger $"Lowest grade of failing student: {Helpers.formatDecimals lowestGrade}"
    }
