using System.ComponentModel.DataAnnotations;
using CampusPlacementManagement.Enums;

namespace CampusPlacementManagement.Models;

public class Student
{
    public int StudentId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    public int PlacementOfficerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RegisterNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    public int GraduationYear { get; set; }

    public decimal CGPA { get; set; }

    public int ActiveBacklogs { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Pending;

    public DateTime? ApprovedAt { get; set; }
}