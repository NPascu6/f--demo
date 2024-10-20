module Program

[<EntryPoint>]
let main argv =
    Student.summarize argv.[0]

    0