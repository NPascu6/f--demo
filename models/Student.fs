module StudentModel

type Grade = { Value: float; Name: string }

type Student =
    { Name: string
      Id: string
      AverageGrade: float
      HighestGrade: Grade
      LowestGrade: Grade }
