using System;

namespace EmployeeManagement.Exceptions;

/// <summary>
/// Domain exception thrown when an operation violates the unique email constraint.
/// </summary>
public class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"An employee with email '{email}' already exists.")
    {
    }
}
