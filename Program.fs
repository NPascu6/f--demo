module Program

[<EntryPoint>]
let main argv =
    let studentFilePath = Helpers.getFileFromPath

    Student.summarize studentFilePath argv
    0
