using System;

namespace EmployeeManagement.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing Employee record.
/// </summary>
public class UpdateEmployeeRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateOnly JoiningDate { get; set; }
    public bool IsActive { get; set; }
}
