module StudentTests

open System
open System.IO
open Xunit
open Logger
open Student

let getTempLogFile () =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\log.txt")

// Helper function to create a temporary test file with student data
let createTestFile (content: string) =
    let filePath = Path.Combine(Path.GetTempPath(), "students-test.txt")
    File.WriteAllText(filePath, content)
    filePath

[<Fact>]
let ``Parsing a valid student row should work correctly`` () =
    // Arrange
    let header = [| "Name"; "ID"; "Math"; "English"; "Science" |]
    let row = "John Doe\t123\t85.00\t90.00\t88.00"

    // Act
    let student = fromString (row, header)

    // Assert
    Assert.Equal("John Doe", student.Name)
    Assert.Equal("123", student.Id)
    Assert.Equal(87.66666666666667, student.AverageGrade, 2)
    Assert.Equal(90.0, student.HighestGrade.Value)
    Assert.Equal(85.0, student.LowestGrade.Value)

[<Fact>]
let ``Parsing a row with missing grades should throw an error`` () =
    // Arrange
    let header = [| "Name"; "ID"; "Math"; "English"; "Science" |]
    let row = "Jane Doe\t124\t85.0\t" // Missing Science grade

    // Act & Assert
    Assert.Throws<FormatException>(fun () -> fromString (row, header) |> ignore)

[<Fact>]
let ``LogSummary should log student details correctly`` () =
    // Arrange
    let logger = FileLogger() :> ILogger
    let logFile = getTempLogFile ()

    let student =
        { Name = "Jane Doe"
          Id = "124"
          AverageGrade = 87.5
          HighestGrade = { Value = 90.0; Name = "Math" }
          LowestGrade = { Value = 85.0; Name = "English" } }

    // Act
    logSummary student logger

    // Assert
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("Student: Jane Doe", logContents)
    Assert.Contains("Average Grade: 87.5", logContents)
    Assert.Contains("Highest Grade: Math: 90", logContents)
    Assert.Contains("Lowest Grade: English: 85", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file

[<Fact>]
let ``Summarize should process valid file correctly`` () =
    // Arrange
    let fileContent =
        "Name\tID\tMath\tEnglish\tScience\nJohn Doe\t123\t85.00\t90.00\t88.00\nJane Doe\t124\t78.00\t92.00\t80.00"

    let filePath = createTestFile (fileContent)

    // Act
    summarize filePath

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("Reading file from:", logContents)
    Assert.Contains("Student: Jane Doe", logContents) // Jane Doe has higher average
    Assert.Contains("Student: John Doe", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file

[<Fact>]
let ``Summarize should log an error for a missing file`` () =
    // Arrange
    let missingFilePath = Path.Combine(Path.GetTempPath(), "non-existent-file.txt")

    // Act
    summarize missingFilePath

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("Please check the path.", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file


[<Fact>]
let ``Summarize should log an error for an empty file`` () =
    // Arrange
    let emptyFile = createTestFile ("")

    // Act
    summarize emptyFile

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("The file is empty. No data to process.", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file


[<Fact>]
let ``Summarize should handle invalid data gracefully`` () =
    // Arrange
    let invalidContent =
        "Name\tID\tMath\tEnglish\tScience\nInvalid Student\t125\tbaddata\t92.00\t80.00"

    let invalidFilePath = createTestFile (invalidContent)

    // Act
    summarize invalidFilePath

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("An error occurred", logContents)
    Assert.Contains("baddata", logContents) // The invalid grade
    Assert.Contains("Invalid row format. Please check the data.", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file


[<Fact>]
let ``print should log to logger when method is logger`` () =
    // Arrange
    let student =
        { Name = "John Doe"
          Id = "123"
          AverageGrade = 87.6
          HighestGrade = { Value = 90.0; Name = "Math" }
          LowestGrade = { Value = 85.0; Name = "English" } }

    // Act
    print student "logger"

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains("Student: John Doe", logContents)

    // Cleanup
    File.WriteAllText(logFile, "") // Clear the file


[<Fact>]
let ``print should write to console when method is console`` () =
    // Arrange
    let student =
        { Name = "John Doe"
          Id = "123"
          AverageGrade = 87.6
          HighestGrade = { Value = 90.0; Name = "Math" }
          LowestGrade = { Value = 85.0; Name = "English" } }

    // Capture console output
    let consoleOutput = Console.Out
    use outputCapture = new StringWriter()
    Console.SetOut(outputCapture)

    // Act
    print student "console"

    // Assert
    let consoleContents = outputCapture.ToString()
    Assert.Contains("Student: John Doe", consoleContents)

    // Restore console output
    Console.SetOut(consoleOutput)

[<Fact>]
let ``fromString should correctly identify highest and lowest grades`` () =
    // Arrange
    let header = [| "Name"; "ID"; "Math"; "English"; "Science" |]
    let row = "John Doe\t123\t95.00\t92.00\t88.00"

    // Act
    let student = fromString (row, header)

    // Assert
    Assert.Equal(95.0, student.HighestGrade.Value)
    Assert.Equal("Math", student.HighestGrade.Name)
    Assert.Equal(88.0, student.LowestGrade.Value)
    Assert.Equal("Science", student.LowestGrade.Name)

[<Fact>]
let ``formString should throw format error when file contents do not have float point or are negative or weird math``
    ()
    =
    // Arrange
    let header = [| "Name"; "ID"; "Math"; "English"; "Science" |]
    let row = "John Doe\t123\t95.0.0\t-92.00\t88.0000"

    // Act & Assert
    Assert.Throws<FormatException>(fun () -> fromString (row, header) |> ignore)

[<Fact>]
let ``fail print with invalid method`` () =
    // Arrange
    let student =
        { Name = "John Doe"
          Id = "123"
          AverageGrade = 87.6
          HighestGrade = { Value = 90.0; Name = "Math" }
          LowestGrade = { Value = 85.0; Name = "English" } }

    // Act & Assert
    Assert.Throws<ArgumentException>(fun () -> print student "invalid" |> ignore)
