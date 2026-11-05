using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces;

public interface IPlacementOfficerRepository
{
    Task AddAsync(PlacementOfficer placementOfficer);

    Task<PlacementOfficer?> GetByIdAsync(int id);

    Task<PlacementOfficer?> GetByUserIdAsync(string userId);

    Task<List<PlacementOfficer>> GetAllAsync();

    Task<List<PlacementOfficer>> GetPendingAsync();

    Task SaveChangesAsync();
}