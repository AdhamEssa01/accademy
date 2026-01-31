using Academy.Application.Contracts.Demo;
using FluentValidation;

namespace Academy.Application.Validation.Demo;

public sealed class CreateDemoRequestValidator : AbstractValidator<CreateDemoRequest>
{
    public CreateDemoRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 50);

        RuleFor(x => x.Age)
            .InclusiveBetween(6, 99);
    }
}