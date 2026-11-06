using CampusPlacementManagement.DTOs.CompanyHR;

namespace CampusPlacementManagement.Services.Interfaces;

public interface ICompanyHRService
{
    Task<int> RegisterAsync(RegisterCompanyHRRequestDto request);

    Task<List<CompanyHRResponseDto>> GetAllAsync();

    Task<List<CompanyHRResponseDto>> GetPendingAsync();

    Task<CompanyHRResponseDto?> GetByIdAsync(int companyHRId);

    Task<CompanyHRResponseDto?> GetMyProfileAsync(string userId);

    Task<bool> ApproveAsync(int companyHRId, string approvedBy);

    Task<bool> RejectAsync(int companyHRId);
}