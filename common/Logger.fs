module Logger

open System
open System.IO

// Define ILogger interface with different log levels
type ILogger =
    abstract member Log: string -> unit
    abstract member LogError: string -> unit
    abstract member LogWarning: string -> unit
    abstract member LogInfo: string -> unit

// Enum to specify log length limits per log type
type LogLengthPerType =
    | Log = 1000
    | LogError = 1500
    | LogWarning = 2000
    | LogInfo = 2500

// FileLogger class implementation
type FileLogger(?logPath) =

    // Default log path if none provided
    let defaultPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\log.txt")

    // Ensure the log path is valid
    let path =
        match logPath with
        | Some p when not (String.IsNullOrWhiteSpace(p)) && File.Exists(p) -> p
        | Some p when String.IsNullOrWhiteSpace(p) -> raise (ArgumentException("Path cannot be empty."))
        | Some _ -> raise (ArgumentException("File does not exist at provided path."))
        | None -> defaultPath

    // Lock object to prevent concurrent writes
    static let logLock = obj ()

    // Private helper method for logging
    let logMessage (msgType: string) (msg: string) (maxLength: int) =
        lock logLock (fun () ->
            // Check if the file exceeds the specified limit for the log type
            if File.Exists(path) && File.ReadAllLines(path).Length > maxLength then
                File.Delete(path)

            // Ensure the file exists
            if not (File.Exists(path)) then
                File.Create(path).Dispose()

            // Write log message
            try
                use fileStream =
                    new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)

                use writer = new StreamWriter(fileStream)
                writer.WriteLine($"{DateTime.Now:s} {msgType}: {msg}")
                writer.Flush()
            with ex ->
                Console.WriteLine($"Failed to write to log file: {ex.Message}"))

    // Implement ILogger methods using the common logMessage function
    interface ILogger with
        member this.Log(msg: string) =
            logMessage "" msg (int LogLengthPerType.Log)

        member this.LogError(msg: string) =
            logMessage "ERROR" msg (int LogLengthPerType.LogError)

        member this.LogWarning(msg: string) =
            logMessage "WARNING" msg (int LogLengthPerType.LogWarning)

        member this.LogInfo(msg: string) =
            logMessage "INFO" msg (int LogLengthPerType.LogInfo)
