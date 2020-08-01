using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SoftUni.Data;
using SoftUni.Models;

namespace SoftUni
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            
        }

        // 03. Employees Full Information
        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Select(e => new
                {
                    e.FirstName,
                    e.MiddleName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} {emp.MiddleName} {emp.JobTitle} {emp.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 04. Employees with Salary Over 50 000
        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(s => s.Salary > 50000)
                .Select(e => new
                {
                    e.FirstName,
                    e.Salary
                })
                .OrderBy(n => n.FirstName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} - {emp.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 05. Employees from Research and Development
        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.Department.Name == "Research and Development")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    DepartmentName = e.Department.Name,
                    e.Salary
                })
                .OrderBy(emp => emp.Salary)
                .ThenByDescending(emp => emp.FirstName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} from {emp.DepartmentName} - ${emp.Salary:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        // 06. Adding a New Address And Updating Employee
        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            {
                AddressText = "Vitoshka 15", TownId = 4
            };

            Employee employee = context
                .Employees
                .FirstOrDefault(n => n.LastName == "Nakov");

            employee.Address = address;

            context.SaveChanges();

            var employees = context
                .Employees
                .Select(e => new
                {
                    e.AddressId,
                    e.Address.AddressText
                })
                .OrderByDescending(e => e.AddressId)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.AddressText}");
            }

            return sb.ToString().TrimEnd();
        }

        // 07. Employees and Projects
        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(e => e.EmployeesProjects
                    .Any(ep => ep.Project.StartDate.Year >= 2001 && 
                                           ep.Project.StartDate.Year <= 2003))
                .Take(10)
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    ManagerFirstName = e.Manager.FirstName,
                    ManagerLastName = e.Manager.LastName,
                    Project = e.EmployeesProjects
                        .Select(ep => new
                        {
                            ProjectName = ep.Project.Name,
                            ProjectStartDate = ep.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                            ProjectEndDate = ep.Project.EndDate.HasValue
                                ? ep.Project
                                    .EndDate
                                    .Value
                                    .ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
                                : "not finished"
                        })
                        .ToList()
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - Manager: {emp.ManagerFirstName} {emp.ManagerLastName}");

                foreach (var project in emp.Project)
                {
                    sb.AppendLine($"--{project.ProjectName} - {project.ProjectStartDate} - {project.ProjectEndDate}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // 08. Addresses by Town
        public static string GetAddressesByTown(SoftUniContext context)
        {
            var addresses = context
                .Addresses
                .Select(a => new
                {
                    Employees = a.Employees.Count,
                    a.AddressText,
                    TownName = a.Town.Name
                })
                .OrderByDescending(a => a.Employees)
                .ThenBy(t => t.TownName)
                .ThenBy(a => a.AddressText)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var address in addresses)
            {
                sb.AppendLine($"{address.AddressText}, {address.TownName} - {address.Employees} employees");
            }

            return sb.ToString().TrimEnd();
        }

        // 09. Employee 147
        public static string GetEmployee147(SoftUniContext context)
        {
            var employee147 = context
                .Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects
                        .Select(p => new
                        {
                            ProjectName = p.Project.Name
                        })
                        .OrderBy(pn => pn.ProjectName)
                        .ToList()
                })
                .FirstOrDefault(e => e.EmployeeId == 147);

            var sb = new StringBuilder();

            sb.AppendLine($"{employee147.FirstName} {employee147.LastName} - {employee147.JobTitle}");

            foreach (var project in employee147.Projects)
            {
                sb.AppendLine($"{project.ProjectName}");
            }

            return sb.ToString().TrimEnd();
        }

        // 10. Departments with More Than 5 Employees
        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var departments = context
                .Departments
                .Where(e => e.Employees.Count > 5)
                .Select(d => new
                {
                    DepartmentName = d.Name,
                    DepartmentManagerFirstName = d.Manager.FirstName,
                    DepartmentManagerLastName = d.Manager.LastName,
                    Employees = d.Employees
                        .Select(n => new
                        {
                            EmployeeFirstName = n.FirstName,
                            EmployeeLastName = n.LastName,
                            EmployeeJob = n.JobTitle
                        })
                        .OrderBy(ef => ef.EmployeeFirstName)
                        .ThenBy(ep => ep.EmployeeLastName)
                        .ToList()
                })
                .ToList();

            var sb = new StringBuilder();

            foreach (var dep in departments)
            {
                sb.AppendLine(
                    $"{dep.DepartmentName} - {dep.DepartmentManagerFirstName} {dep.DepartmentManagerLastName}");

                foreach (var emp in dep.Employees)
                {
                    sb.AppendLine($"{emp.EmployeeFirstName} {emp.EmployeeLastName} - {emp.EmployeeJob}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        // 11. Find Latest 10 Projects
        public static string GetLatestProjects(SoftUniContext context)
        {
            var projects = context
                .Projects
                .Select(p => new
                {
                    ProjectName = p.Name,
                    Description = p.Description,
                    StartDate = p.StartDate
                })
                .OrderByDescending(sd => sd.StartDate)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var project in projects.OrderBy(p => p.ProjectName))
            {
                sb.AppendLine($"{project.ProjectName}");
                sb.AppendLine($"{project.Description}");
                sb.AppendLine($"{project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)}");
            }

            return sb.ToString().TrimEnd();
        }

        // 12. Increase Salaries
        public static string IncreaseSalaries(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(d => d.Department.Name == "Engineering"
                            || d.Department.Name == "Tool Design"
                            || d.Department.Name == "Marketing"
                            || d.Department.Name == "Information Services")
                .ToList();

            foreach (var emp in employees)
            {
                emp.Salary += emp.Salary * 0.12m;
            }

            context.SaveChanges();

            var employeesWithIncreasedSalary = context
                .Employees
                .Where(d => d.Department.Name == "Engineering"
                            || d.Department.Name == "Tool Design"
                            || d.Department.Name == "Marketing"
                            || d.Department.Name == "Information Services")
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.Salary
                })
                .OrderBy(n => n.FirstName)
                .ThenBy(n => n.LastName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employeesWithIncreasedSalary)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} (${emp.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        // 13. Find Employees by First Name Starting with "Sa"
        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var employees = context
                .Employees
                .Where(n => n.FirstName.StartsWith("Sa"))
                .Select(e => new
                {
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    e.Salary
                })
                .OrderBy(n => n.FirstName)
                .ThenBy(n => n.LastName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.FirstName} {emp.LastName} - {emp.JobTitle} - (${emp.Salary:f2})");
            }

            return sb.ToString().TrimEnd();
        }

        // 14. Delete Project by Id
        public static string DeleteProjectById(SoftUniContext context)
        {
            var project = context.Projects.Where(p => p.ProjectId == 2).FirstOrDefault();

            var employeesWhoReferenceProject = context
                .EmployeesProjects
                .Where(ep => ep.ProjectId == 2)
                .ToList();

            context.EmployeesProjects.RemoveRange(employeesWhoReferenceProject);

            context.Projects.Remove(project);

            context.SaveChanges();

            var projects = context
                .Projects
                .Select(p => new
                {
                    p.Name
                })
                .Take(10)
                .ToList();

            var sb = new StringBuilder();

            foreach (var proj in projects)
            {
                sb.AppendLine($"{proj.Name}");
            }

            return sb.ToString().TrimEnd();
        }

        // 15. Remove Town
        public static string RemoveTown(SoftUniContext context)
        {
            Town townToDel = context
                .Towns
                .FirstOrDefault(t => t.Name == "Seattle");

            var addressesToDel = context
                .Addresses
                .Where(a => a.TownId == townToDel.TownId);

            int addressesCount = addressesToDel.Count();

            var employeesOnDeletedAddresses = context
                .Employees
                .Where(e => addressesToDel
                    .Any(a => a.AddressId == e.AddressId));

            foreach (Employee emp in employeesOnDeletedAddresses)
            {
                    emp.AddressId = null;
            }

            foreach (var address in addressesToDel)
            {
                context.Addresses.Remove(address);
            }

            context.Towns.Remove(townToDel);
            context.SaveChanges();

            return $"{addressesCount} addresses in Seattle were deleted";
        }
    }
}
