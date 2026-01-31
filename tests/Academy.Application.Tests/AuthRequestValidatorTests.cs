using Academy.Application.Contracts.Auth;
using Academy.Application.Validation.Auth;
using Xunit;

namespace Academy.Application.Tests;

public class AuthRequestValidatorTests
{
    [Fact]
    public void RegisterRequest_Invalid_When_Empty()
    {
        var validator = new RegisterRequestValidator();
        var request = new RegisterRequest();

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LoginRequest_Invalid_When_ShortPassword()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "short"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void RefreshRequest_Invalid_When_Empty()
    {
        var validator = new RefreshRequestValidator();
        var request = new RefreshRequest();

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GoogleLoginRequest_Invalid_When_Empty()
    {
        var validator = new GoogleLoginRequestValidator();
        var request = new GoogleLoginRequest();

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
