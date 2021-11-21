using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Module4HW4.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.Json;

namespace Module4HW4
{
    public class Starter
    {
        public void Run()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var dbOptionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            var connectionString = configuration.GetConnectionString("Module4HW5");
            dbOptionsBuilder.UseSqlServer(connectionString, i => i.CommandTimeout(20));
            var applicationContext = new ApplicationContext(dbOptionsBuilder.Options);

            var query1 = applicationContext.Client
                .Include(i => i.Projects)
                    .ThenInclude(i => i.EmployeeProject)
                        .ThenInclude(i => i.Employee)
                .ToList();

            var query2 = applicationContext.Employee
                .Select(i => new { birthHiredDiff = EF.Functions.DateDiffDay(i.DateOfBirth, i.HiredDate), hiredTodayDiff = EF.Functions.DateDiffDay(i.DateOfBirth, DateTime.UtcNow) }).ToList();

            var project = applicationContext.Project.First(i => i.Budget <= 20000);
            project.Budget += 2000;
            var employeeProject = applicationContext.EmployeeProject.Where(i => i.ProjectId == project.ProjectId).ToList();
            foreach (var empProject in employeeProject)
            {
                empProject.Rate = project.Budget;
            }

            applicationContext.Project.Update(project);
            applicationContext.EmployeeProject.UpdateRange(employeeProject);

            applicationContext.Employee.Add(new DataAccess.Entities.Employee { TitleId = 2, EmployeeProject = employeeProject, DateOfBirth = new DateTime(1995, 01, 20), FirstName = "Andrey", LastName = "Kudryashov", HiredDate = new DateTime(2019, 11, 10), OfficeId = 1 });
            applicationContext.SaveChanges();

            var employee = applicationContext.Employee.Where(i => i.LastName == "Kudryashov").ToList();
            applicationContext.Employee.RemoveRange(employee);

            var titleName = applicationContext.Employee
                .GroupBy(i => i.Title.Name)
                .Select(i => i.Key)
                .Where(i => !EF.Functions.Like(i, "%[^a]%"))
                .ToList();

            applicationContext.SaveChanges();
        }
    }
}