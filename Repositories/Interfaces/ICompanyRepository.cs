using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task AddAsync(Company company);

    Task<List<Company>> GetAllAsync();

    Task<Company?> GetByIdAsync(int companyId);

    Task<List<Company>> GetPendingAsync();

    Task<bool> CompanyEmailExistsAsync(
        string companyEmail,
        int? excludeCompanyId = null);

    Task SaveChangesAsync();
}