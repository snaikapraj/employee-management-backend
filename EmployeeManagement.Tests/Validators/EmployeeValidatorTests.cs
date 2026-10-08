using System;
using EmployeeManagement.DTOs;
using EmployeeManagement.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace EmployeeManagement.Tests.Validators;

public class EmployeeValidatorTests
{
    private readonly CreateEmployeeRequestValidator _createValidator = new();
    private readonly UpdateEmployeeRequestValidator _updateValidator = new();

    [Fact]
    public void CreateEmployeeRequestValidator_ValidRequest_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Software Engineer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15),
            IsActive = true
        };

        // Act & Assert
        var result = _createValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void CreateEmployeeRequestValidator_InvalidSalary_ShouldHaveValidationError(decimal invalidSalary)
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Software Engineer",
            Salary = invalidSalary,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act & Assert
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Salary)
              .WithErrorMessage("Salary must be greater than 0.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email-format")]
    [InlineData("test@")]
    public void CreateEmployeeRequestValidator_InvalidEmail_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = invalidEmail,
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Software Engineer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act & Assert
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateEmployeeRequestValidator_EmptyFirstName_ShouldHaveValidationError()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Software Engineer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act & Assert
        var result = _createValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
              .WithErrorMessage("First name is required.");
    }

    [Fact]
    public void UpdateEmployeeRequestValidator_InvalidSalary_ShouldHaveValidationError()
    {
        // Arrange
        var request = new UpdateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Software Engineer",
            Salary = 0m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act & Assert
        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Salary);
    }
}
