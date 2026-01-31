using Academy.Application.Contracts.Students;
using FluentValidation;

namespace Academy.Application.Validation.Students;

public sealed class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Notes)
            .MaximumLength(800)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
