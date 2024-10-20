module LoggerTests

open System.IO
open Xunit
open Logger
open System

// Helper function to create a temporary path for testing
let getTempLogFile () =
    //if file doesn't exist create it
    if
        not (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\test-log.txt")))
    then
        File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\test-log.txt"))
        |> ignore

    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\test-log.txt")

// Test Log writes correctly to the log file
[<Fact>]
let ``Log writes correctly to the log file`` () =
    // Arrange
    let logger = FileLogger(getTempLogFile ()) :> ILogger
    let logMessage = "Test log message"
    File.WriteAllText(getTempLogFile (), "")

    // Act
    logger.Log(logMessage)

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains(logMessage, logContents)

    //add output to console
    Console.WriteLine(logContents)

[<Fact>]
let ``Log writes 1000 lines then deletes file and creates a new one`` () =
    // Arrange
    let logger = FileLogger(getTempLogFile ()) :> ILogger
    let logMessage = "Test log message 2"
    File.WriteAllText(getTempLogFile (), "")

    // Act
    for i in 0..1001 do
        logger.Log(logMessage)

    // Assert
    let logFile = getTempLogFile ()
    let logContents = File.ReadAllText(logFile)
    Assert.Contains(logMessage, logContents)
    Assert.Equal(2, logContents.Split('\n').Length)

[<Fact>]
let ``Logger throws error File does not exist at provided path.`` () =
    // Assert
    let ex =
        Assert.Throws<ArgumentException>(fun () -> FileLogger("C:\\test-log.txt") :> ILogger |> ignore)

    Assert.Equal("File does not exist at provided path.", ex.Message)
    Assert.True(ex.Message.Contains("File does not exist at provided path."))
    Assert.IsType<ArgumentException>(ex)

[<Fact>]
let ``Logger throws error Path cannot be empty.`` () =
    // Assert
    let ex =
        Assert.Throws<ArgumentException>(fun () -> FileLogger("") :> ILogger |> ignore)

    Assert.Equal("Path cannot be empty.", ex.Message)
    Assert.True(ex.Message.Contains("Path cannot be empty."))
    Assert.IsType<ArgumentException>(ex)
