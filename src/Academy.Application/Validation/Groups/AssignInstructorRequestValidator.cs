using Academy.Application.Contracts.Groups;
using FluentValidation;

namespace Academy.Application.Validation.Groups;

public sealed class AssignInstructorRequestValidator : AbstractValidator<AssignInstructorRequest>
{
    public AssignInstructorRequestValidator()
    {
        RuleFor(x => x.InstructorUserId)
            .NotEmpty();
    }
}
