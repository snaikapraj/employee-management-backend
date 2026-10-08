using System;
using System.Threading.Tasks;
using EmployeeManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Data;

/// <summary>
/// Handles initial database migrations and seeding for development.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (await context.Employees.AnyAsync())
        {
            return; // Seed data already present
        }

        var employees = new[]
        {
            new Employee
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
            },
            new Employee
            {
                FirstName = "Priya",
                LastName = "Patel",
                Email = "priya.patel@example.com",
                Phone = "9876543211",
                Department = "Human Resources",
                Designation = "HR Specialist",
                Salary = 55000m,
                JoiningDate = new DateOnly(2024, 6, 1),
                IsActive = true
            },
            new Employee
            {
                FirstName = "Rahul",
                LastName = "Deshmukh",
                Email = "rahul.d@example.com",
                Phone = "9876543212",
                Department = "Finance",
                Designation = "Financial Analyst",
                Salary = 62000m,
                JoiningDate = new DateOnly(2023, 11, 20),
                IsActive = true
            }
        };

        await context.Employees.AddRangeAsync(employees);
        await context.SaveChangesAsync();
    }
}
