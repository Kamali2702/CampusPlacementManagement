
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagement.DTOs.Student;

public class UpdateStudentRequestDto
{
    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    [Range(2000, 2100)]
    public int GraduationYear { get; set; }

    [Range(typeof(decimal), "0", "10")]
    public decimal CGPA { get; set; }

    [Range(0, int.MaxValue)]
    public int ActiveBacklogs { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}
