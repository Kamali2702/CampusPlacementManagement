
using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces;

public interface ICollegeService
{
    Task<CollegeResponseDto> RegisterCollegeAsync(
        RegisterCollegeRequestDto request);

    Task<List<CollegeResponseDto>> GetAllCollegesAsync();

    Task<CollegeResponseDto?> GetCollegeByIdAsync(int collegeId);

    Task<List<CollegeResponseDto>> GetPendingCollegesAsync();

    Task<bool> ApproveCollegeAsync(int collegeId);

    Task<bool> RejectCollegeAsync(int collegeId);

    Task<bool> SuspendCollegeAsync(int collegeId);

    Task<bool> UpdateCollegeAsync(
        int collegeId,
        UpdateCollegeRequestDto request);
}
