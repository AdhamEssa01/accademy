using Academy.Application.Contracts.Behavior;
using FluentValidation;

namespace Academy.Application.Validation.Behavior;

public sealed class CreateBehaviorEventRequestValidator : AbstractValidator<CreateBehaviorEventRequest>
{
    public CreateBehaviorEventRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Points)
            .InclusiveBetween(-20, 20);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Note)
            .MaximumLength(500);
    }
}
