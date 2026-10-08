using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EmployeeManagement.DTOs;

namespace EmployeeManagement.Services.Interfaces;

/// <summary>
/// Service interface defining business logic operations for Employee management.
/// </summary>
public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
