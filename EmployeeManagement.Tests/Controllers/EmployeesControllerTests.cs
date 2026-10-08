using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmployeeManagement.Controllers;
using EmployeeManagement.DTOs;
using EmployeeManagement.Services.Interfaces;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Controllers;

public class EmployeesControllerTests
{
    private readonly Mock<IEmployeeService> _mockService = new();
    private readonly Mock<IValidator<CreateEmployeeRequest>> _mockCreateValidator = new();
    private readonly Mock<IValidator<UpdateEmployeeRequest>> _mockUpdateValidator = new();

    private EmployeesController CreateController()
    {
        var controller = new EmployeesController(
            _mockService.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithEmployees()
    {
        // Arrange
        var employees = new List<EmployeeDto>
        {
            new EmployeeDto { EmployeeId = 1, FirstName = "Aarav", LastName = "Sharma", Email = "aarav@example.com" }
        };

        _mockService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(employees);

        var controller = CreateController();

        // Act
        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var model = okResult.Value.Should().BeAssignableTo<IEnumerable<EmployeeDto>>().Subject;
        model.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_WhenExists_ShouldReturnOkWithEmployee()
    {
        // Arrange
        var employee = new EmployeeDto { EmployeeId = 1, FirstName = "Aarav", LastName = "Sharma", Email = "aarav@example.com" };

        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(1, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var model = okResult.Value.Should().BeOfType<EmployeeDto>().Subject;
        model.EmployeeId.Should().Be(1);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmployeeDto?)null);

        var controller = CreateController();

        // Act
        var result = await controller.GetById(999, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValid_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Developer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        var createdDto = new EmployeeDto { EmployeeId = 1, FirstName = "Aarav", Email = "aarav@example.com" };

        _mockCreateValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockService.Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        var controller = CreateController();

        // Act
        var result = await controller.Create(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(EmployeesController.GetById));
        createdResult.RouteValues!["id"].Should().Be(1);
    }

    [Fact]
    public async Task Update_WhenValid_ShouldReturnOkWithUpdatedEmployee()
    {
        // Arrange
        var request = new UpdateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav.sharma@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Senior Developer",
            Salary = 75000m,
            JoiningDate = new DateOnly(2025, 1, 15),
            IsActive = true
        };

        var updatedDto = new EmployeeDto { EmployeeId = 1, FirstName = "Aarav", Email = "aarav.sharma@example.com" };

        _mockUpdateValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _mockService.Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        var controller = CreateController();

        // Act
        var result = await controller.Update(1, request, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var model = okResult.Value.Should().BeOfType<EmployeeDto>().Subject;
        model.Email.Should().Be("aarav.sharma@example.com");
    }

    [Fact]
    public async Task Delete_WhenExists_ShouldReturnNoContent()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var controller = CreateController();

        // Act
        var result = await controller.Delete(1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockService.Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = CreateController();

        // Act
        var result = await controller.Delete(999, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
