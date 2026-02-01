namespace Academy.Application.Contracts.Exams;

public sealed class UpdateExamQuestionsRequest
{
    public List<ExamQuestionItemRequest> Questions { get; set; } = new();
}
