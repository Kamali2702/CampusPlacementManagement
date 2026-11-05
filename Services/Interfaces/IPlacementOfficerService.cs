using CampusPlacementManagement.DTOs;

namespace CampusPlacementManagement.Services.Interfaces;

public interface IPlacementOfficerService
{
    Task<int> RegisterAsync(RegisterPlacementOfficerRequestDto request);

    Task<List<PlacementOfficerResponseDto>> GetAllAsync();

    Task<List<PlacementOfficerResponseDto>> GetPendingAsync();

    Task<PlacementOfficerResponseDto?> GetByIdAsync(int id);
    Task<PlacementOfficerResponseDto?> GetMyProfileAsync(string userId);

    Task<bool> ApproveAsync(int id, string approvedBy);

    Task<bool> RejectAsync(int id);

    Task<bool> SuspendAsync(int id);
}