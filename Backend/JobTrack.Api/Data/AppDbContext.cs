using JobTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTrack.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<JobApplication> JobApplications
        => Set<JobApplication>();

    public DbSet<StatusHistory> StatusHistory
        => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<JobApplication>()
            .HasOne(application => application.User)
            .WithMany(user => user.JobApplications)
            .HasForeignKey(application => application.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .HasOne(application => application.Company)
            .WithMany(company => company.JobApplications)
            .HasForeignKey(application => application.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatusHistory>()
            .HasOne(history => history.JobApplication)
            .WithMany(application => application.StatusHistoryEntries)
            .HasForeignKey(history => history.JobApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobApplication>()
            .Property(application => application.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<StatusHistory>()
            .Property(history => history.PreviousStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<StatusHistory>()
            .Property(history => history.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<JobApplication>()
            .HasIndex(application => new
            {
                application.UserId,
                application.Status
            });

        modelBuilder.Entity<StatusHistory>()
            .HasIndex(history => new
            {
                history.JobApplicationId,
                history.ChangedAt
            });
    }
}