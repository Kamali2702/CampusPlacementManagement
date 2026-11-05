
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(
        ApplicationUser user,
        IList<string> roles);
}
