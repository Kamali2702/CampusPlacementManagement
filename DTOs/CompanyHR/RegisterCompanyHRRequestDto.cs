using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.DTOs.CompanyHR;

public class RegisterCompanyHRRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int CompanyId { get; set; }

    [MaxLength(100)]
    public string? Designation { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}