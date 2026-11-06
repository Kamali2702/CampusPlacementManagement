using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models;

public class Company
{
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string CompanyEmail { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Website { get; set; }

    [Required]
    [MaxLength(100)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    public string? Description { get; set; }

    public CompanyStatus Status { get; set; } = CompanyStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}