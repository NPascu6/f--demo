module Logger

open System
open System.IO

type ILogger =
    abstract member Log: string -> unit
    abstract member LogError: string -> unit

type FileLogger() =
    static let path =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Files\\log.txt")

    static let logLock = obj ()

    interface ILogger with
        member this.Log(msg: string) =
            lock logLock (fun () ->
                if File.Exists(path) then
                    if File.ReadAllLines(path).Length > 1500 then
                        File.Delete(path)

                try
                    use fileStream =
                        new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)

                    use writer = new StreamWriter(fileStream)
                    writer.WriteLine($"{DateTime.Now:s} {msg}")
                    writer.Flush()
                with ex ->
                    Console.WriteLine($"Failed to write to log file: {ex.Message}"))

        member this.LogError(msg: string) =
            lock logLock (fun () ->
                if File.Exists(path) then
                    if File.ReadAllLines(path).Length > 1500 then
                        File.Delete(path)

                try
                    use fileStream =
                        new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite)

                    use writer = new StreamWriter(fileStream)
                    writer.WriteLine($"{DateTime.Now:s} ERROR: {msg}")
                    writer.Flush()
                with ex ->
                    Console.WriteLine($"Failed to write to log file: {ex.Message}"))
