using System;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Data;
using EmployeeManagement.DTOs;
using EmployeeManagement.Entities;
using EmployeeManagement.Exceptions;
using EmployeeManagement.Repositories;
using EmployeeManagement.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EmployeeManagement.Tests.Services;

public class EmployeeServiceTests
{
    private static ApplicationDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Mock<ILogger<EmployeeService>> GetLoggerMock()
    {
        return new Mock<ILogger<EmployeeService>>();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEmployees()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        context.Employees.AddRange(
            new Employee { EmployeeId = 1, FirstName = "Aarav", LastName = "Sharma", Email = "aarav@example.com", Phone = "9876543210", Department = "Engineering", Designation = "Developer", Salary = 50000m, JoiningDate = new DateOnly(2025, 1, 15) },
            new Employee { EmployeeId = 2, FirstName = "Priya", LastName = "Patel", Email = "priya@example.com", Phone = "9876543211", Department = "HR", Designation = "Specialist", Salary = 55000m, JoiningDate = new DateOnly(2024, 6, 1) }
        );
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(e => e.FirstName).Should().Contain(new[] { "Aarav", "Priya" });
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ShouldReturnEmployee()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        context.Employees.Add(new Employee
        {
            EmployeeId = 1,
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Developer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        });
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Aarav");
        result.Email.Should().Be("aarav@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenValidRequest_ShouldCreateAndReturnEmployee()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Developer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15),
            IsActive = true
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("aarav@example.com");
        result.EmployeeId.Should().BeGreaterThan(0);

        var dbEmployee = await context.Employees.FindAsync(result.EmployeeId);
        dbEmployee.Should().NotBeNull();
        dbEmployee!.FirstName.Should().Be("Aarav");
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateEmail_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        context.Employees.Add(new Employee
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Eng",
            Designation = "Dev",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        });
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        var request = new CreateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "AARAV@example.com", // Case insensitive check test
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Developer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act
        Func<Task> act = async () => await service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage("*aarav@example.com*");
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeExists_ShouldUpdateAndReturnEmployee()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var employee = new Employee
        {
            EmployeeId = 1,
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Engineering",
            Designation = "Developer",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        var updateRequest = new UpdateEmployeeRequest
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

        // Act
        var result = await service.UpdateAsync(1, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("aarav.sharma@example.com");
        result.Designation.Should().Be("Senior Developer");
        result.Salary.Should().Be(75000m);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        var updateRequest = new UpdateEmployeeRequest
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

        // Act
        var result = await service.UpdateAsync(999, updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateEmailWithAnotherEmployee_ShouldThrowDuplicateEmailException()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        context.Employees.AddRange(
            new Employee { EmployeeId = 1, FirstName = "Aarav", LastName = "Sharma", Email = "aarav@example.com", Phone = "9876543210", Department = "Eng", Designation = "Dev", Salary = 50000m, JoiningDate = new DateOnly(2025, 1, 15) },
            new Employee { EmployeeId = 2, FirstName = "Priya", LastName = "Patel", Email = "priya@example.com", Phone = "9876543211", Department = "HR", Designation = "Specialist", Salary = 55000m, JoiningDate = new DateOnly(2024, 6, 1) }
        );
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        var updateRequest = new UpdateEmployeeRequest
        {
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "priya@example.com", // Belongs to EmployeeId = 2
            Phone = "9876543210",
            Department = "Eng",
            Designation = "Dev",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        };

        // Act
        Func<Task> act = async () => await service.UpdateAsync(1, updateRequest);

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_ShouldReturnTrueAndRemove()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        context.Employees.Add(new Employee
        {
            EmployeeId = 1,
            FirstName = "Aarav",
            LastName = "Sharma",
            Email = "aarav@example.com",
            Phone = "9876543210",
            Department = "Eng",
            Designation = "Dev",
            Salary = 50000m,
            JoiningDate = new DateOnly(2025, 1, 15)
        });
        await context.SaveChangesAsync();

        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        (await context.Employees.AnyAsync(e => e.EmployeeId == 1)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
        var repo = new EmployeeRepository(context);
        var service = new EmployeeService(repo, GetLoggerMock().Object);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
