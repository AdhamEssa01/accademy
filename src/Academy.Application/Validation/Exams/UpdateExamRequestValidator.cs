using Academy.Application.Contracts.Exams;
using Academy.Domain;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class UpdateExamRequestValidator : AbstractValidator<UpdateExamRequest>
{
    public UpdateExamRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.DurationMinutes)
            .InclusiveBetween(1, 240);
    }
}
