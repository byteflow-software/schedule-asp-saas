using FluentAssertions;
using Scheduly.Application.Features.Customers.Commands.CreateCustomer;

namespace Scheduly.UnitTests.Application.Features.Customers;

public class CreateCustomerCommandValidatorTests
{
    private readonly CreateCustomerCommandValidator _sut = new();

    [Fact]
    public async Task Validate_ValidCommand_IsValid()
    {
        var cmd = new CreateCustomerCommand("John Doe", "john@test.com", "11999999999", "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyName_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("", "john@test.com", "11999999999", "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public async Task Validate_NameTooLong_IsInvalid()
    {
        var cmd = new CreateCustomerCommand(new string('A', 201), "john@test.com", null, "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public async Task Validate_InvalidEmail_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("John", "not-email", null, "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validate_EmptyEmail_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("John", "", null, "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Validate_PhoneTooLong_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("John", "john@test.com", new string('1', 21), "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public async Task Validate_NullPhone_IsValid()
    {
        var cmd = new CreateCustomerCommand("John", "john@test.com", null, "12345678901");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyCpfCnpj_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("John", "john@test.com", null, "");
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CpfCnpj");
    }

    [Fact]
    public async Task Validate_CpfCnpjTooLong_IsInvalid()
    {
        var cmd = new CreateCustomerCommand("John", "john@test.com", null, new string('1', 19));
        var result = await _sut.ValidateAsync(cmd);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CpfCnpj");
    }
}
