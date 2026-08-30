using System.Text;
using InternshipPortal.Data;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Admin,UniversitySupervisor")]
    public class ComplianceReportController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public ComplianceReportController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            ComplianceReportViewModel filters)
        {
            var model = await BuildReportAsync(filters);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Print(
            string? search,
            string? university,
            string? complianceStatus)
        {
            var filters = new ComplianceReportViewModel
            {
                Search = search,
                University = university,
                ComplianceStatus = complianceStatus
            };

            var model = await BuildReportAsync(filters);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ExportCsv(
            string? search,
            string? university,
            string? complianceStatus)
        {
            var filters = new ComplianceReportViewModel
            {
                Search = search,
                University = university,
                ComplianceStatus = complianceStatus
            };

            var model = await BuildReportAsync(filters);

            var csv = new StringBuilder();

            csv.AppendLine(
                "Student,University,Specialization," +
                "Internship,Company,Start Date,End Date," +
                "Required Hours,Logged Hours,Approved Hours," +
                "Weekly Reports,Approved Reports," +
                "Compliance Rate,Status");

            foreach (var student in model.Students)
            {
                csv.AppendLine(
                    $"{EscapeCsv(student.StudentName)}," +
                    $"{EscapeCsv(student.University)}," +
                    $"{EscapeCsv(student.Specialization)}," +
                    $"{EscapeCsv(student.InternshipTitle)}," +
                    $"{EscapeCsv(student.CompanyName)}," +
                    $"{student.StartDate:yyyy-MM-dd}," +
                    $"{student.ExpectedEndDate:yyyy-MM-dd}," +
                    $"{student.RequiredHours}," +
                    $"{student.LoggedHours:0.##}," +
                    $"{student.ApprovedHours:0.##}," +
                    $"{student.WeeklyReportsCount}," +
                    $"{student.ApprovedReportsCount}," +
                    $"{student.HoursCompletionRate:0.0}%," +
                    $"{EscapeCsv(student.ComplianceStatus)}");
            }

            var preamble = Encoding.UTF8.GetPreamble();
            var csvBytes = Encoding.UTF8.GetBytes(
                csv.ToString());

            var fileBytes = preamble
                .Concat(csvBytes)
                .ToArray();

            var fileName =
                $"Training-Compliance-{DateTime.Now:yyyyMMdd-HHmm}.csv";

            return File(
                fileBytes,
                "text/csv; charset=utf-8",
                fileName);
        }

        private async Task<ComplianceReportViewModel>
            BuildReportAsync(
                ComplianceReportViewModel filters)
        {
            var currentUserId =
                userManager.GetUserId(User);

            var isAdmin =
                User.IsInRole("Admin");

            var enrollmentsQuery =
                context.TrainingEnrollments
                    .AsNoTracking()
                    .Include(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Student)
                    .Include(enrollment =>
                        enrollment.InternshipApplication)
                    .ThenInclude(application =>
                        application.Internship)
                    .ThenInclude(internship =>
                        internship.Company)
                    .Include(enrollment =>
                        enrollment.HourEntries)
                    .Include(enrollment =>
                        enrollment.WeeklyReports)
                    .AsQueryable();

            if (!isAdmin)
            {
                enrollmentsQuery =
                    enrollmentsQuery.Where(enrollment =>
                        enrollment.UniversitySupervisorUserId ==
                            currentUserId);
            }

            var enrollments =
                await enrollmentsQuery.ToListAsync();

            var allStudents = enrollments
                .Select(enrollment =>
                {
                    var application =
                        enrollment.InternshipApplication;

                    var student =
                        application.Student;

                    var internship =
                        application.Internship;

                    var loggedHours =
                        enrollment.HourEntries
                            .Sum(entry =>
                                Convert.ToDouble(entry.Hours));

                    var approvedHours =
                        enrollment.HourEntries
                            .Where(entry =>
                                entry.Status.ToString()
                                    .Equals(
                                        "Approved",
                                        StringComparison.OrdinalIgnoreCase))
                            .Sum(entry =>
                                Convert.ToDouble(entry.Hours));

                    var approvedReports =
                        enrollment.WeeklyReports
                            .Count(report =>
                                report.Status.ToString()
                                    .Equals(
                                        "Approved",
                                        StringComparison.OrdinalIgnoreCase));

                    var completionRate =
                        enrollment.RequiredHours > 0
                            ? Math.Min(
                                approvedHours * 100.0 /
                                enrollment.RequiredHours,
                                100)
                            : 0;

                    var isUniversityApproved =
                        enrollment.UniversityApprovedAt.HasValue;

                    var isTrainingCompleted =
                        enrollment.CompletedAt.HasValue;

                    var isOverdue =
                        !isTrainingCompleted &&
                        enrollment.ExpectedEndDate.Date <
                            DateTime.Today;

                    var complianceStatus =
                        GetComplianceStatus(
                            isUniversityApproved,
                            isTrainingCompleted,
                            isOverdue,
                            completionRate,
                            enrollment.WeeklyReports.Count);

                    var complianceScore =
                        CalculateComplianceScore(
                            isUniversityApproved,
                            completionRate,
                            enrollment.WeeklyReports.Count,
                            approvedReports,
                            isOverdue);

                    return new ComplianceStudentViewModel
                    {
                        TrainingEnrollmentId =
                            enrollment.Id,

                        StudentName =
                            student.FullName,

                        University =
                            student.University,

                        Faculty =
                            student.Faculty,

                        Specialization =
                            student.Specialization,

                        InternshipTitle =
                            internship.Title,

                        CompanyName =
                            internship.Company.Name,

                        StartDate =
                            enrollment.StartDate,

                        ExpectedEndDate =
                            enrollment.ExpectedEndDate,

                        RequiredHours =
                            enrollment.RequiredHours,

                        LoggedHours =
                            loggedHours,

                        ApprovedHours =
                            approvedHours,

                        WeeklyReportsCount =
                            enrollment.WeeklyReports.Count,

                        ApprovedReportsCount =
                            approvedReports,

                        HoursCompletionRate =
                            Math.Round(
                                completionRate,
                                1),

                        IsUniversityApproved =
                            isUniversityApproved,

                        IsTrainingCompleted =
                            isTrainingCompleted,

                        IsOverdue =
                            isOverdue,

                        ComplianceStatus =
                            complianceStatus,

                        ComplianceScore =
                            complianceScore
                    };
                })
                .OrderBy(student =>
                    GetStatusOrder(
                        student.ComplianceStatus))
                .ThenBy(student =>
                    student.StudentName)
                .ToList();

            var students = allStudents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();

                students = students.Where(student =>
                    student.StudentName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    student.InternshipTitle.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    student.CompanyName.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) ||
                    student.Specialization.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(
                filters.University))
            {
                students = students.Where(student =>
                    student.University.Equals(
                        filters.University,
                        StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(
                filters.ComplianceStatus))
            {
                students = students.Where(student =>
                    student.ComplianceStatus.Equals(
                        filters.ComplianceStatus,
                        StringComparison.OrdinalIgnoreCase));
            }

            filters.Students = students.ToList();

            filters.IsAdminView = isAdmin;

            filters.AvailableUniversities =
                allStudents
                    .Select(student =>
                        student.University)
                    .Where(university =>
                        !string.IsNullOrWhiteSpace(university))
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .OrderBy(university =>
                        university)
                    .ToList();

            filters.TotalEnrollments =
                allStudents.Count;

            filters.UniversityApproved =
                allStudents.Count(student =>
                    student.IsUniversityApproved);

            filters.FullyCompliant =
                allStudents.Count(student =>
                    student.ComplianceStatus ==
                        "Compliant");

            filters.NeedsAttention =
                allStudents.Count(student =>
                    student.ComplianceStatus ==
                        "Needs Attention" ||
                    student.ComplianceStatus ==
                        "Overdue" ||
                    student.ComplianceStatus ==
                        "Pending Approval");

            filters.TotalRequiredHours =
                allStudents.Sum(student =>
                    student.RequiredHours);

            filters.TotalApprovedHours =
                allStudents.Sum(student =>
                    student.ApprovedHours);

            filters.OverallComplianceRate =
                filters.TotalRequiredHours > 0
                    ? Math.Round(
                        Math.Min(
                            filters.TotalApprovedHours *
                            100.0 /
                            filters.TotalRequiredHours,
                            100),
                        1)
                    : 0;

            return filters;
        }

        private static string GetComplianceStatus(
            bool isUniversityApproved,
            bool isTrainingCompleted,
            bool isOverdue,
            double completionRate,
            int weeklyReportsCount)
        {
            if (!isUniversityApproved)
            {
                return "Pending Approval";
            }

            if (isTrainingCompleted &&
                completionRate >= 100)
            {
                return "Compliant";
            }

            if (isOverdue)
            {
                return "Overdue";
            }

            if (completionRate >= 75 &&
                weeklyReportsCount > 0)
            {
                return "On Track";
            }

            return "Needs Attention";
        }

        private static int CalculateComplianceScore(
            bool isUniversityApproved,
            double completionRate,
            int totalReports,
            int approvedReports,
            bool isOverdue)
        {
            var approvalScore =
                isUniversityApproved ? 20 : 0;

            var hoursScore =
                Math.Min(
                    completionRate * 0.5,
                    50);

            var reportScore =
                totalReports > 0
                    ? approvedReports * 20.0 /
                      totalReports
                    : 0;

            var scheduleScore =
                isOverdue ? 0 : 10;

            var score =
                approvalScore +
                hoursScore +
                reportScore +
                scheduleScore;

            return Math.Min(
                Convert.ToInt32(Math.Round(score)),
                100);
        }

        private static int GetStatusOrder(
            string status)
        {
            return status switch
            {
                "Overdue" => 1,
                "Needs Attention" => 2,
                "Pending Approval" => 3,
                "On Track" => 4,
                "Compliant" => 5,
                _ => 6
            };
        }

        private static string EscapeCsv(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var escapedValue =
                value.Replace("\"", "\"\"");

            return $"\"{escapedValue}\"";
        }
    }
}