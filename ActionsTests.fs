module ActionsTests

open Xunit
open Actions
open Logger
open System
open System.IO

let logFilePath =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\test-log.txt")

// Lock object for concurrency control on file access
let fileLock = obj ()

// Helper to write logs asynchronously
let asyncWriteToFile (filePath: string) (logs: string list) =
    async {
        // Ensure file writing is done in a thread-safe manner using a lock
        lock fileLock (fun () ->
            if File.Exists(filePath) then
                if File.ReadAllLines(filePath).Length > 500 then
                    File.Delete(filePath)

            File.AppendAllLines(filePath, logs)
            use writer = File.AppendText(filePath)
            writer.WriteLine("----------------------------------\n")
            writer.Flush())
    }

// Mocking Logger to capture log messages during tests
type MockLogger() =
    let mutable logs = []

    interface ILogger with
        member this.Log(msg: string) = logs <- msg :: logs
        member this.LogError(msg: string) = logs <- msg :: logs

    member this.GetLogs() = List.rev logs

[<Fact>]
let ``Summarize Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    summarize (mockLogger, sampleData)
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("File has", logs.[0])
    Assert.Contains("students", logs.[1])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students That Passed Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsThatPassed (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Number of students that passed: 1", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students That Failed Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsThatFailed (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Number of students that failed: 1", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Print Score Per Student Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    printScorePerStudent (mockLogger, sampleData, sampleData.[0])
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Student number: 1 and Name: Alice", logs.[0])
    Assert.Contains("Average score: 55.5", logs.[1])
    Assert.Contains("Highest grade: Score with 55.5", logs.[2])
    Assert.Contains("Student number: 2 and Name: Bob", logs.[3])
    Assert.Contains("Average score: 45.0", logs.[4])
    Assert.Contains("Highest grade: Score with 45.0", logs.[5])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students With Scholarship Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsWithScholarship (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Average Grade Per Class Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    averageGradePerClass (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Average grade per class:", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Highest Grade Column Name Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getHighestGradeColumnName (mockLogger, sampleData.[1], sampleData.[0])
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Highest grade: Score with 55.5", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Print Main Score Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]

    printMainScore (mockLogger, sampleData.[1], sampleData.[0], 1)
    |> Async.RunSynchronously

    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Student number: 1 and Name: Alice", logs.[0])
    Assert.Contains("Average score: 55.5", logs.[1])
    Assert.Contains("Highest grade: Score with 55.5", logs.[2])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students That Passed Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsThatPassed (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Number of students that passed: 1", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students That Failed Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsThatFailed (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Number of students that failed: 1", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Students With Scholarship Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getStudentsWithScholarship (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Average Grade Per Class Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    averageGradePerClass (mockLogger, sampleData) |> Async.RunSynchronously

    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Average grade per class:", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Average Grade Per Passing Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    averageGradePerPassing (mockLogger, sampleData) |> Async.RunSynchronously
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Average grade per passing student:", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Get Highest Grade Column Name Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]
    getHighestGradeColumnName (mockLogger, sampleData.[1], sampleData.[0])
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Highest grade: Score with 55.5", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Print Main Score Async Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t55.5"; "Bob\t2\t45.0" |]

    printMainScore (mockLogger, sampleData.[1], sampleData.[0], 1)
    |> Async.RunSynchronously

    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Student number: 1 and Name: Alice", logs.[0])
    Assert.Contains("Average score: 55.5", logs.[1])
    Assert.Contains("Highest grade: Score with 55.5", logs.[2])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

[<Fact>]
let ``Print Main Score Async returns nothing Test`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = [| "Name\tID\tScore"; "Alice\t1\t45.5"; "Bob\t2\t45.0" |]

    printMainScore (mockLogger, sampleData.[1], sampleData.[0], 1)
    |> Async.RunSynchronously

    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Student number: 1 and Name: Alice", logs.[0])
    Assert.Contains("Average score: 45.5", logs.[1])
    Assert.Contains("Highest grade: Score with 45.5", logs.[2])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously

//write 20 more test with the same structure as the above tests that test null out of bounds file does not exist etc
[<Fact>]
let ``Summarize Test Null Data`` () =
    // Create a new instance of MockLogger for this test
    let mockLogger = MockLogger()

    let sampleData = null
    summarize (mockLogger, sampleData)
    let logs = mockLogger.GetLogs()

    // Assertions
    Assert.True(logs.Length > 0, "Logs should not be empty")
    Assert.Contains("Data is null", logs.[0])

    // Write logs asynchronously
    asyncWriteToFile logFilePath logs |> Async.RunSynchronously
