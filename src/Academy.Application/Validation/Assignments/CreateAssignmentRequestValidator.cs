using Academy.Application.Contracts.Assignments;
using FluentValidation;

namespace Academy.Application.Validation.Assignments;

public sealed class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleForEach(x => x.TargetStudentIds)
            .NotEmpty()
            .When(x => x.TargetStudentIds is { Count: > 0 });
    }
}
