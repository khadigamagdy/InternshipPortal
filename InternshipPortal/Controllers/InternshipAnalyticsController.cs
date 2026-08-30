using InternshipPortal.Data;
using InternshipPortal.Models.Enums;
using InternshipPortal.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Controllers
{
    [Authorize(Roles = "Admin,Company")]
    public class InternshipAnalyticsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;

        public InternshipAnalyticsController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId =
                userManager.GetUserId(User);

            var isAdmin = User.IsInRole("Admin");

            var companyName = "Platform Analytics";
            int? companyId = null;

            if (!isAdmin)
            {
                var company = await context.Companies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(company =>
                        company.UserId == currentUserId);

                if (company == null)
                {
                    TempData["Error"] =
                        "Please complete your company profile first.";

                    return RedirectToAction(
                        "Profile",
                        "Company");
                }

                companyId = company.Id;
                companyName = company.Name;
            }

            var internshipsQuery = context.Internships
                .AsNoTracking()
                .Include(internship =>
                    internship.Company)
                .Include(internship =>
                    internship.Applications)
                .ThenInclude(application =>
                    application.Evaluation)
                .AsQueryable();

            if (companyId.HasValue)
            {
                internshipsQuery =
                    internshipsQuery.Where(internship =>
                        internship.CompanyId ==
                            companyId.Value);
            }

            var internships =
                await internshipsQuery.ToListAsync();

            var applications = internships
                .SelectMany(internship =>
                    internship.Applications)
                .ToList();

            var totalApplications = applications.Count;

            var pendingApplications = applications.Count(
                application =>
                    application.Status ==
                        ApplicationStatus.Pending ||
                    application.Status ==
                        ApplicationStatus.UnderReview ||
                    application.Status ==
                        ApplicationStatus.InterviewScheduled);

            var acceptedApplications = applications.Count(
                application =>
                    application.Status ==
                        ApplicationStatus.Accepted ||
                    application.Status ==
                        ApplicationStatus.Completed);

            var rejectedApplications = applications.Count(
                application =>
                    application.Status ==
                        ApplicationStatus.Rejected);

            var completedTrainings = applications.Count(
                application =>
                    application.Status ==
                        ApplicationStatus.Completed);

            var reviewedApplications = applications
                .Where(application =>
                    application.ReviewedAt.HasValue &&
                    application.ReviewedAt.Value >=
                        application.AppliedAt)
                .ToList();

            var averageReviewHours =
                reviewedApplications.Any()
                    ? reviewedApplications.Average(application =>
                        (application.ReviewedAt!.Value -
                         application.AppliedAt).TotalHours)
                    : 0;

            var ratings = applications
                .Where(application =>
                    application.Evaluation != null)
                .Select(application =>
                    application.Evaluation!.Rating)
                .ToList();

            var averageRating = ratings.Any()
                ? ratings.Average()
                : 0;

            var performance =
                internships.Select(internship =>
                {
                    var internshipApplications =
                        internship.Applications.ToList();

                    var internshipTotal =
                        internshipApplications.Count;

                    var internshipAccepted =
                        internshipApplications.Count(
                            application =>
                                application.Status ==
                                    ApplicationStatus.Accepted ||
                                application.Status ==
                                    ApplicationStatus.Completed);

                    var internshipRejected =
                        internshipApplications.Count(
                            application =>
                                application.Status ==
                                    ApplicationStatus.Rejected);

                    var internshipCompleted =
                        internshipApplications.Count(
                            application =>
                                application.Status ==
                                    ApplicationStatus.Completed);

                    var internshipRatings =
                        internshipApplications
                            .Where(application =>
                                application.Evaluation != null)
                            .Select(application =>
                                application.Evaluation!.Rating)
                            .ToList();

                    var internshipAverageRating =
                        internshipRatings.Any()
                            ? internshipRatings.Average()
                            : 0;

                    var internshipReviewed =
                        internshipApplications
                            .Where(application =>
                                application.ReviewedAt.HasValue &&
                                application.ReviewedAt.Value >=
                                    application.AppliedAt)
                            .ToList();

                    var internshipReviewHours =
                        internshipReviewed.Any()
                            ? internshipReviewed.Average(
                                application =>
                                    (application.ReviewedAt!.Value -
                                     application.AppliedAt)
                                    .TotalHours)
                            : 0;

                    var acceptanceRate =
                        CalculatePercentage(
                            internshipAccepted,
                            internshipTotal);

                    var completionRate =
                        CalculatePercentage(
                            internshipCompleted,
                            internshipAccepted);

                    var qualityScore =
                        CalculateQualityScore(
                            internshipTotal,
                            acceptanceRate,
                            completionRate,
                            internshipAverageRating,
                            internshipReviewHours);

                    return new InternshipPerformanceViewModel
                    {
                        InternshipId = internship.Id,
                        InternshipTitle = internship.Title,
                        CompanyName =
                            internship.Company.Name,
                        IsActive = internship.IsActive,
                        TotalApplications =
                            internshipTotal,
                        AcceptedApplications =
                            internshipAccepted,
                        RejectedApplications =
                            internshipRejected,
                        CompletedTrainings =
                            internshipCompleted,
                        RemainingPositions =
                            internship.AvailablePositions,
                        AcceptanceRate =
                            acceptanceRate,
                        CompletionRate =
                            completionRate,
                        AverageRating =
                            internshipAverageRating,
                        AverageReviewHours =
                            internshipReviewHours,
                        QualityScore =
                            qualityScore
                    };
                })
                .OrderByDescending(item =>
                    item.QualityScore)
                .ThenByDescending(item =>
                    item.TotalApplications)
                .ToList();

            var bestInternship =
                performance.FirstOrDefault(item =>
                    item.TotalApplications > 0);

            var monthlyTrends =
                BuildMonthlyTrends(applications);

            var model = new InternshipAnalyticsViewModel
            {
                IsAdminView = isAdmin,
                DashboardOwnerName = companyName,
                TotalInternships = internships.Count,
                ActiveInternships = internships.Count(
                    internship =>
                        internship.IsActive),
                TotalApplications = totalApplications,
                PendingApplications = pendingApplications,
                AcceptedApplications = acceptedApplications,
                RejectedApplications = rejectedApplications,
                CompletedTrainings = completedTrainings,

                AcceptanceRate =
                    CalculatePercentage(
                        acceptedApplications,
                        totalApplications),

                RejectionRate =
                    CalculatePercentage(
                        rejectedApplications,
                        totalApplications),

                CompletionRate =
                    CalculatePercentage(
                        completedTrainings,
                        acceptedApplications),

                AverageRating = averageRating,
                AverageReviewHours = averageReviewHours,

                BestInternshipTitle =
                    bestInternship?.InternshipTitle ??
                    "No data yet",

                BestInternshipScore =
                    bestInternship?.QualityScore ?? 0,

                InternshipPerformance = performance,
                MonthlyTrends = monthlyTrends
            };

            return View(model);
        }

        private static List<MonthlyApplicationTrendViewModel>
            BuildMonthlyTrends(
                List<Models.InternshipApplication> applications)
        {
            var trends =
                new List<MonthlyApplicationTrendViewModel>();

            var currentMonth = new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                1);

            for (var index = 5; index >= 0; index--)
            {
                var monthStart =
                    currentMonth.AddMonths(-index);

                var monthEnd =
                    monthStart.AddMonths(1);

                var monthlyApplications =
                    applications.Where(application =>
                        application.AppliedAt >= monthStart &&
                        application.AppliedAt < monthEnd)
                    .ToList();

                trends.Add(
                    new MonthlyApplicationTrendViewModel
                    {
                        MonthName =
                            monthStart.ToString("MMM yyyy"),

                        Applications =
                            monthlyApplications.Count,

                        Accepted =
                            monthlyApplications.Count(
                                application =>
                                    application.Status ==
                                        ApplicationStatus.Accepted ||
                                    application.Status ==
                                        ApplicationStatus.Completed),

                        Completed =
                            monthlyApplications.Count(
                                application =>
                                    application.Status ==
                                        ApplicationStatus.Completed)
                    });
            }

            return trends;
        }

        private static double CalculatePercentage(
            int value,
            int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return Math.Round(
                value * 100.0 / total,
                1);
        }

        private static int CalculateQualityScore(
            int totalApplications,
            double acceptanceRate,
            double completionRate,
            double averageRating,
            double averageReviewHours)
        {
            if (totalApplications == 0)
            {
                return 0;
            }

            var demandScore =
                Math.Min(totalApplications * 2, 20);

            var acceptanceScore =
                Math.Min(
                    acceptanceRate * 0.2,
                    20);

            var completionScore =
                Math.Min(
                    completionRate * 0.3,
                    30);

            var ratingScore =
                averageRating > 0
                    ? averageRating / 5 * 25
                    : 0;

            double responseScore;

            if (averageReviewHours <= 0)
            {
                responseScore = 0;
            }
            else if (averageReviewHours <= 24)
            {
                responseScore = 5;
            }
            else if (averageReviewHours <= 72)
            {
                responseScore = 3;
            }
            else
            {
                responseScore = 1;
            }

            var totalScore =
                demandScore +
                acceptanceScore +
                completionScore +
                ratingScore +
                responseScore;

            return Math.Min(
                Convert.ToInt32(Math.Round(totalScore)),
                100);
        }
    }
}