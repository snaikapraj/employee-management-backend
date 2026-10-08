using System.Collections.Generic;

namespace EmployeeManagement.DTOs;

/// <summary>
/// Standard error response format returned by API for unhandled or validation errors.
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }
}
