using CampusPlacementManagement.DTOs.Company;

namespace CampusPlacementManagement.Services.Interfaces;

public interface ICompanyService
{
    Task<int> RegisterAsync(RegisterCompanyRequestDto request);

    Task<List<CompanyResponseDto>> GetAllAsync();

    Task<List<CompanyResponseDto>> GetPendingAsync();

    Task<CompanyResponseDto?> GetByIdAsync(int companyId);

    Task<bool> ApproveAsync(int companyId);

    Task<bool> RejectAsync(int companyId);

    Task<bool> SuspendAsync(int companyId);
}