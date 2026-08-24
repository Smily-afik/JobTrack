using System.ComponentModel.DataAnnotations;

namespace JobTrack.Api.Models;

public class JobApplication
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? JobUrl { get; set; }

    public ApplicationStatus Status { get; set; }
        = ApplicationStatus.Interested;

    public DateOnly? ApplicationDeadline { get; set; }

    public DateTime? AppliedAt { get; set; }

    [MaxLength(5000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = null!;

    public Company Company { get; set; } = null!;

    public ICollection<StatusHistory> StatusHistoryEntries { get; set; }
        = new List<StatusHistory>();
}