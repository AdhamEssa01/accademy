using Academy.Application.Contracts.Branches;
using FluentValidation;

namespace Academy.Application.Validation.Branches;

public sealed class UpdateBranchRequestValidator : AbstractValidator<UpdateBranchRequest>
{
    public UpdateBranchRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .MaximumLength(400);
    }
}
