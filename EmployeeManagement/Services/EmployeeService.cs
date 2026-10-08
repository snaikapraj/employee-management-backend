using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmployeeManagement.DTOs;
using EmployeeManagement.Entities;
using EmployeeManagement.Exceptions;
using EmployeeManagement.Repositories.Interfaces;
using EmployeeManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Services;

/// <summary>
/// Implements business logic for managing employee records via EmployeeRepository.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository repository, ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving all employees");
        var employees = await _repository.GetAllAsync(cancellationToken);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving employee with ID {EmployeeId}", id);
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new employee with email {Email}", request.Email);

        var emailExists = await _repository.ExistsByEmailAsync(request.Email, null, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning("Attempted to create employee with duplicate email {Email}", request.Email);
            throw new DuplicateEmailException(request.Email);
        }

        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            Department = request.Department.Trim(),
            Designation = request.Designation.Trim(),
            Salary = request.Salary,
            JoiningDate = request.JoiningDate,
            IsActive = request.IsActive
        };

        var createdEmployee = await _repository.AddAsync(employee, cancellationToken);

        _logger.LogInformation("Successfully created employee with ID {EmployeeId}", createdEmployee.EmployeeId);
        return MapToDto(createdEmployee);
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating employee with ID {EmployeeId}", id);

        var employee = await _repository.GetByIdAsync(id, cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found for update", id);
            return null;
        }

        var emailExists = await _repository.ExistsByEmailAsync(request.Email, id, cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning("Attempted to update employee {EmployeeId} with duplicate email {Email}", id, request.Email);
            throw new DuplicateEmailException(request.Email);
        }

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim().ToLowerInvariant();
        employee.Phone = request.Phone.Trim();
        employee.Department = request.Department.Trim();
        employee.Designation = request.Designation.Trim();
        employee.Salary = request.Salary;
        employee.JoiningDate = request.JoiningDate;
        employee.IsActive = request.IsActive;

        await _repository.UpdateAsync(employee, cancellationToken);

        _logger.LogInformation("Successfully updated employee with ID {EmployeeId}", id);
        return MapToDto(employee);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting employee with ID {EmployeeId}", id);

        var employee = await _repository.GetByIdAsync(id, cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Employee with ID {EmployeeId} not found for deletion", id);
            return false;
        }

        await _repository.DeleteAsync(employee, cancellationToken);

        _logger.LogInformation("Successfully deleted employee with ID {EmployeeId}", id);
        return true;
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Department = employee.Department,
            Designation = employee.Designation,
            Salary = employee.Salary,
            JoiningDate = employee.JoiningDate,
            IsActive = employee.IsActive
        };
    }
}
