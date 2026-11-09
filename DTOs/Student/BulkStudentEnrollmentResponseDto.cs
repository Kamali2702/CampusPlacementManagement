namespace CampusPlacementManagement.DTOs.Student;

public class BulkStudentEnrollmentResponseDto
{
    public int TotalRows { get; set; }
    public int SuccessfulEnrollments { get; set; }
    public int FailedEnrollments { get; set; }

    public List<BulkStudentEnrollmentRowResultDto> Results { get; set; }
        = new();
}