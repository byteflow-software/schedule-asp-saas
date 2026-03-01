using FluentAssertions;
using Scheduly.Application.Features.Auth.Commands.Login;

namespace Scheduly.UnitTests.Application.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ValidCommand_IsValid()
    {
        var result = await _sut.ValidateAsync(new LoginCommand("user@test.com", "password123"));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyEmail_IsInvalid()
    {
        var result = await _sut.ValidateAsync(new LoginCommand("", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_InvalidEmail_IsInvalid()
    {
        var result = await _sut.ValidateAsync(new LoginCommand("not-an-email", "password123"));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmptyPassword_IsInvalid()
    {
        var result = await _sut.ValidateAsync(new LoginCommand("user@test.com", ""));
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
