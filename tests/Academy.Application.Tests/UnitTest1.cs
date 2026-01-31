using Academy.Application.Contracts.Demo;
using Academy.Application.Validation.Demo;
using Xunit;

namespace Academy.Application.Tests;

public class CreateDemoRequestValidatorTests
{
    [Fact]
    public void Validator_Flags_Invalid_Request()
    {
        var validator = new CreateDemoRequestValidator();
        var request = new CreateDemoRequest
        {
            Name = "A",
            Age = 3
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Validator_Allows_Valid_Request()
    {
        var validator = new CreateDemoRequestValidator();
        var request = new CreateDemoRequest
        {
            Name = "Valid Name",
            Age = 20
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }
}