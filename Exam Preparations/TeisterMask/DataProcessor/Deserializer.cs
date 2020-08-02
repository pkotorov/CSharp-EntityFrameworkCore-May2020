namespace TeisterMask.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

    using Data;
    using System.Xml;
    using TeisterMask.DataProcessor.ImportDto;
    using System.Xml.Serialization;
    using System.IO;
    using TeisterMask.Data.Models;
    using System.Text;
    using System.Globalization;
    using TeisterMask.Data.Models.Enums;
    using Newtonsoft.Json;
    using Microsoft.EntityFrameworkCore.Internal;
    using System.Linq;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedProject
            = "Successfully imported project - {0} with {1} tasks.";

        private const string SuccessfullyImportedEmployee
            = "Successfully imported employee - {0} with {1} tasks.";

        public static string ImportProjects(TeisterMaskContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportProjectsDto[]), new XmlRootAttribute("Projects"));
            
            using (StringReader sr = new StringReader(xmlString))
            {
                ImportProjectsDto[] projectsDto = (ImportProjectsDto[])xmlSerializer.Deserialize(sr);

                List<Project> projects = new List<Project>();
                List<Task> tasks = new List<Task>();

                foreach (var projectDto in projectsDto)
                {
                    if (!IsValid(projectDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    DateTime projectOpenDate;
                    bool isValidPorjectOpenDate = DateTime
                        .TryParseExact(projectDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out projectOpenDate);

                    DateTime? projectDueDate;

                    if (!string.IsNullOrEmpty(projectDto.DueDate) && !string.IsNullOrWhiteSpace(projectDto.DueDate))
                    {
                        projectDueDate = DateTime.ParseExact(projectDto.DueDate,
                            "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        projectDueDate = null;
                    }

                    if (!isValidPorjectOpenDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    Project project = new Project
                    {
                        Name = projectDto.Name,
                        OpenDate = projectOpenDate,
                        DueDate = projectDueDate,
                    };

                    int tasksCount = 0;

                    foreach (var taskDto in projectDto.Tasks)
                    {
                        DateTime taskOpenDate;
                        bool isValidTaskOpenDate = DateTime
                            .TryParseExact(taskDto.OpenDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out taskOpenDate);


                        DateTime taskDueDate;
                        bool isValidTaskDueDate = DateTime
                            .TryParseExact(taskDto.DueDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out taskDueDate);

                        if (!IsValid(taskDto))
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (!isValidTaskOpenDate || !isValidTaskDueDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (taskOpenDate < projectOpenDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        if (taskDueDate > projectDueDate)
                        {
                            sb.AppendLine(ErrorMessage);
                            continue;
                        }

                        Task task = new Task
                        {
                            Name = taskDto.Name,
                            OpenDate = taskOpenDate,
                            DueDate = taskDueDate,
                            ExecutionType = (ExecutionType)taskDto.ExecutionType,
                            LabelType = (LabelType)taskDto.LabelType,
                            Project = project
                        };

                        tasks.Add(task);
                        tasksCount++;
                    }

                    projects.Add(project);
                    sb.AppendLine(string.Format(SuccessfullyImportedProject, project.Name, tasksCount));
                }

                context.Projects.AddRange(projects);
                context.Tasks.AddRange(tasks);
                context.SaveChanges();

                return sb.ToString().TrimEnd();
            }
        }

        public static string ImportEmployees(TeisterMaskContext context, string jsonString)
        {
            var employeesDto = JsonConvert.DeserializeObject<ImportEmployeesDto[]>(jsonString);

            StringBuilder sb = new StringBuilder();

            List<Employee> employees = new List<Employee>();
            List<EmployeeTask> employeeTasks = new List<EmployeeTask>();

            int tasksCount = 0;

            foreach (var emp in employeesDto)
            {
                if (!IsValid(emp))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                Employee employee = new Employee
                {
                    Username = emp.Username,
                    Email = emp.Email,
                    Phone = emp.Phone
                };

                employees.Add(employee);

                foreach (var task in emp.Tasks.Distinct())
                {
                    if (!context.Tasks.Any(t => t.Id == task))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    EmployeeTask employeeTask = new EmployeeTask
                    {
                        TaskId = task,
                        Employee = employee
                    };

                    tasksCount++;
                    employeeTasks.Add(employeeTask);
                }

                sb.AppendLine(string.Format(SuccessfullyImportedEmployee, emp.Username, tasksCount));
                tasksCount = 0;
            }

            context.Employees.AddRange(employees);
            context.EmployeesTasks.AddRange(employeeTasks);

            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}