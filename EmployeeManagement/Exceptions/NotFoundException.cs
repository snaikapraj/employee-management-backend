using System;

namespace EmployeeManagement.Exceptions;

/// <summary>
/// Domain exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
