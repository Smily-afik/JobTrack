using System.ComponentModel.DataAnnotations;

namespace JobTrack.Api.DTOs.Companies;

public class CreateCompanyRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }
}

public class UpdateCompanyRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }
}

public class CompanyResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Website { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }
}