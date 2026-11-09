namespace CampusPlacementManagement.DTOs.Student;

public class BulkStudentEnrollmentRowResultDto
{
    public int RowNumber { get; set; }
    public string RegisterNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}