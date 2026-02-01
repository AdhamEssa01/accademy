using Academy.Application.Contracts.Exams;
using FluentValidation;

namespace Academy.Application.Validation.Exams;

public sealed class CreateExamAssignmentRequestValidator : AbstractValidator<CreateExamAssignmentRequest>
{
    public CreateExamAssignmentRequestValidator()
    {
        RuleFor(x => x.OpenAtUtc)
            .NotEmpty();

        RuleFor(x => x.CloseAtUtc)
            .NotEmpty();

        RuleFor(x => x.AttemptsAllowed)
            .InclusiveBetween(1, 5);

        RuleFor(x => x)
            .Must(x => x.GroupId.HasValue ^ x.StudentId.HasValue)
            .WithMessage("Either GroupId or StudentId must be provided.");

        RuleFor(x => x)
            .Must(x => x.CloseAtUtc >= x.OpenAtUtc)
            .WithMessage("CloseAtUtc must be after OpenAtUtc.");
    }
}
