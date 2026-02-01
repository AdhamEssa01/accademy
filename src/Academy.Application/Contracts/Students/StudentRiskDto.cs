namespace Academy.Application.Contracts.Students;

public sealed class StudentRiskDto
{
    public Guid StudentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public int Absences { get; set; }

    public int BehaviorPoints { get; set; }

    public decimal AverageEvaluationScore { get; set; }

    public bool IsAtRisk { get; set; }
}
