
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Enums;
using Microsoft.AspNetCore.Http;

namespace CampusPlacementManagement.Services.Interfaces;

public interface IStudentService
{
    Task<StudentResponseDto> EnrollAsync(
        EnrollStudentRequestDto request,
        string placementOfficerUserId);

    Task<BulkStudentEnrollmentResponseDto> BulkEnrollAsync(
        IFormFile file,
        string placementOfficerUserId);

    Task<List<StudentResponseDto>> GetAllAsync(
        string userId,
        bool isSuperAdmin,
        string? search,
        string? department,
        int? graduationYear,
        StudentStatus? status,
        int pageNumber,
        int pageSize);

    Task<List<StudentResponseDto>> GetPendingAsync(
        string placementOfficerUserId);

    Task<StudentResponseDto?> GetByIdAsync(
        int studentId,
        string userId,
        bool isSuperAdmin,
        bool isStudent);

    Task<bool> ApproveAsync(
        int studentId,
        string placementOfficerUserId);

    Task<bool> RejectAsync(
        int studentId,
        string placementOfficerUserId);

    Task<bool> SuspendAsync(
        int studentId,
        string userId,
        bool isSuperAdmin);

    Task<bool> UpdateAsync(
        int studentId,
        string userId,
        bool isSuperAdmin,
        bool isStudent,
        UpdateStudentRequestDto request);

    Task<StudentResponseDto?> GetMyProfileAsync(string userId);
}
