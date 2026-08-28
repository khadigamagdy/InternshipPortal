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

        protected override void OnModelCreating(ModelBuilder builder)
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
                .HasForeignKey(internship => internship.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Internship>()
                .Property(internship => internship.Salary)
                .HasPrecision(10, 2);

            builder.Entity<InternshipApplication>()
                .HasOne(application => application.Student)
                .WithMany(student => student.Applications)
                .HasForeignKey(application => application.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InternshipApplication>()
                .HasOne(application => application.Internship)
                .WithMany(internship => internship.Applications)
                .HasForeignKey(application => application.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InternshipApplication>()
                .HasIndex(application => new
                {
                    application.StudentId,
                    application.InternshipId
                })
                .IsUnique();

            builder.Entity<Evaluation>()
                .HasOne(evaluation => evaluation.InternshipApplication)
                .WithOne(application => application.Evaluation)
                .HasForeignKey<Evaluation>(
                    evaluation =>
                        evaluation.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Evaluation>()
                .HasIndex(evaluation =>
                    evaluation.InternshipApplicationId)
                .IsUnique();

            builder.Entity<Notification>()
                .HasOne(notification => notification.User)
                .WithMany()
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SavedInternship>()
                .HasOne(saved => saved.Student)
                .WithMany()
                .HasForeignKey(saved => saved.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SavedInternship>()
                .HasOne(saved => saved.Internship)
                .WithMany()
                .HasForeignKey(saved => saved.InternshipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SavedInternship>()
                .HasIndex(saved => new
                {
                    saved.StudentId,
                    saved.InternshipId
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

            builder.Entity<Interview>()
                .HasIndex(interview => new
                {
                    interview.InternshipApplicationId,
                    interview.ScheduledAt
                });

            builder.Entity<ApplicationStatusHistory>()
                .HasOne(history =>
                    history.InternshipApplication)
                .WithMany(application =>
                    application.StatusHistory)
                .HasForeignKey(history =>
                    history.InternshipApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationStatusHistory>()
                .HasOne(history => history.ChangedByUser)
                .WithMany()
                .HasForeignKey(history =>
                    history.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationStatusHistory>()
                .HasIndex(history => new
                {
                    history.InternshipApplicationId,
                    history.ChangedAt
                });
        }
    }
}