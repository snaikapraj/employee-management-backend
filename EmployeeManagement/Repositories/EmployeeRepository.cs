using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmployeeManagement.Data;
using EmployeeManagement.Entities;
using EmployeeManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Repositories;

/// <summary>
/// Implements data access operations for Employee entity using Entity Framework Core.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .OrderBy(e => e.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, int? excludeEmployeeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLower();

        if (excludeEmployeeId.HasValue)
        {
            return await _context.Employees
                .AnyAsync(e => e.Email.ToLower() == normalizedEmail && e.EmployeeId != excludeEmployeeId.Value, cancellationToken);
        }

        return await _context.Employees
            .AnyAsync(e => e.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
