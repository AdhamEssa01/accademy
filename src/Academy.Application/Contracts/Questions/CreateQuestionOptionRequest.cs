namespace Academy.Application.Contracts.Questions;

public sealed class CreateQuestionOptionRequest
{
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }
}
