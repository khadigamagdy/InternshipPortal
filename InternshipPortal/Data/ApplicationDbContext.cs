using InternshipPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternshipPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<Internship> Internships { get; set; }

        public DbSet<InternshipApplication> InternshipApplications { get; set; }

        public DbSet<Evaluation> Evaluations { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<SavedInternship> SavedInternships { get; set; }

        public DbSet<Interview> Interviews { get; set; }

        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories
        {
            get;
            set;
        }

        public DbSet<TrainingEnrollment> TrainingEnrollments { get; set; }

        public DbSet<TrainingHourEntry> TrainingHourEntries { get; set; }

        public DbSet<WeeklyReport> WeeklyReports { get; set; }

        public DbSet<StudentPreference> StudentPreferences { get; set; }

        public DbSet<SkillDevelopmentPlan> SkillDevelopmentPlans { get; set; }

        public DbSet<SkillPlanItem> SkillPlanItems { get; set; }

        public DbSet<StudentPortfolio> StudentPortfolios { get; set; }

        public DbSet<PortfolioProject> PortfolioProjects { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Student>()
                .HasIndex(student => student.UserId)
                .IsUnique();

            builder.Entity<Company>()
                .HasIndex(company => company.UserId)
                .IsUnique();

            builder.Entity<Student>()
                .HasOne(student => student.User)
                .WithMany()
                .HasForeignKey(student => student.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Company>()
                .HasOne(company => company.User)
                .WithMany()
                .HasForeignKey(company => company.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Internship>()
                .HasOne(internship => internship.Company)
                .WithMany(company => company.Internships)
                .HasForeignKey(internship =>
                    internship.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Internship>()
                .Property(internship => internship.Salary)
                .HasPrecision(10, 2);

            builder.Entity<InternshipApplication>()
                .HasOne(application => application.Student)
                .WithMany(student => student.Applications)
                .HasForeignKey(application =>
                    application.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InternshipApplication>()
                .HasOne(application =>
                    application.Internship)
                .WithMany(internship =>
                    internship.Applications)
                .HasForeignKey(application =>
                    application.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InternshipApplication>()
                .HasIndex(application => new
                {
                    application.StudentId,
                    application.InternshipId
                })
                .IsUnique();

            builder.Entity<Evaluation>()
                .HasOne(evaluation =>
                    evaluation.InternshipApplication)
                .WithOne(application =>
                    application.Evaluation)
                .HasForeignKey<Evaluation>(evaluation =>
                    evaluation.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Evaluation>()
                .HasIndex(evaluation =>
                    evaluation.InternshipApplicationId)
                .IsUnique();

            builder.Entity<Notification>()
                .HasOne(notification => notification.User)
                .WithMany()
                .HasForeignKey(notification =>
                    notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SavedInternship>()
                .HasOne(savedInternship =>
                    savedInternship.Student)
                .WithMany(student =>
                    student.SavedInternships)
                .HasForeignKey(savedInternship =>
                    savedInternship.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedInternship>()
                .HasOne(savedInternship =>
                    savedInternship.Internship)
                .WithMany()
                .HasForeignKey(savedInternship =>
                    savedInternship.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SavedInternship>()
                .HasIndex(savedInternship => new
                {
                    savedInternship.StudentId,
                    savedInternship.InternshipId
                })
                .IsUnique();

            builder.Entity<Interview>()
                .HasOne(interview =>
                    interview.InternshipApplication)
                .WithMany(application =>
                    application.Interviews)
                .HasForeignKey(interview =>
                    interview.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationStatusHistory>()
                .HasOne(history =>
                    history.InternshipApplication)
                .WithMany(application =>
                    application.StatusHistory)
                .HasForeignKey(history =>
                    history.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationStatusHistory>()
                .HasOne(history =>
                    history.ChangedByUser)
                .WithMany()
                .HasForeignKey(history =>
                    history.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrainingEnrollment>()
                .HasOne(enrollment =>
                    enrollment.InternshipApplication)
                .WithOne(application =>
                    application.TrainingEnrollment)
                .HasForeignKey<TrainingEnrollment>(
                    enrollment =>
                        enrollment.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TrainingEnrollment>()
                .HasIndex(enrollment =>
                    enrollment.InternshipApplicationId)
                .IsUnique();

            builder.Entity<TrainingEnrollment>()
                .HasOne(enrollment =>
                    enrollment.UniversitySupervisorUser)
                .WithMany()
                .HasForeignKey(enrollment =>
                    enrollment.UniversitySupervisorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TrainingHourEntry>()
                .HasOne(entry =>
                    entry.TrainingEnrollment)
                .WithMany(enrollment =>
                    enrollment.HourEntries)
                .HasForeignKey(entry =>
                    entry.TrainingEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WeeklyReport>()
                .HasOne(report =>
                    report.TrainingEnrollment)
                .WithMany(enrollment =>
                    enrollment.WeeklyReports)
                .HasForeignKey(report =>
                    report.TrainingEnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentPreference>()
                .HasOne(preference =>
                    preference.Student)
                .WithOne(student =>
                    student.Preference)
                .HasForeignKey<StudentPreference>(
                    preference =>
                        preference.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentPreference>()
                .HasIndex(preference =>
                    preference.StudentId)
                .IsUnique();

            builder.Entity<StudentPreference>()
                .Property(preference =>
                    preference.MinimumSalary)
                .HasPrecision(10, 2);

            builder.Entity<SkillDevelopmentPlan>()
                .HasOne(plan => plan.Student)
                .WithMany(student =>
                    student.SkillDevelopmentPlans)
                .HasForeignKey(plan =>
                    plan.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SkillDevelopmentPlan>()
                .HasOne(plan => plan.Internship)
                .WithMany()
                .HasForeignKey(plan =>
                    plan.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SkillDevelopmentPlan>()
                .HasIndex(plan => new
                {
                    plan.StudentId,
                    plan.InternshipId
                })
                .IsUnique();

            builder.Entity<SkillPlanItem>()
                .HasOne(item =>
                    item.SkillDevelopmentPlan)
                .WithMany(plan =>
                    plan.Items)
                .HasForeignKey(item =>
                    item.SkillDevelopmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SkillPlanItem>()
                .HasIndex(item => new
                {
                    item.SkillDevelopmentPlanId,
                    item.SkillName
                })
                .IsUnique();

            builder.Entity<StudentPortfolio>()
                .HasOne(portfolio =>
                    portfolio.Student)
                .WithOne(student =>
                    student.Portfolio)
                .HasForeignKey<StudentPortfolio>(
                    portfolio =>
                        portfolio.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentPortfolio>()
                .HasIndex(portfolio =>
                    portfolio.StudentId)
                .IsUnique();

            builder.Entity<StudentPortfolio>()
                .HasIndex(portfolio =>
                    portfolio.PortfolioSlug)
                .IsUnique();

            builder.Entity<PortfolioProject>()
                .HasOne(project =>
                    project.StudentPortfolio)
                .WithMany(portfolio =>
                    portfolio.Projects)
                .HasForeignKey(project =>
                    project.StudentPortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PortfolioProject>()
                .HasIndex(project => new
                {
                    project.StudentPortfolioId,
                    project.Title
                });
        }
    }
}