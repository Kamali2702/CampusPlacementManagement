
namespace CampusPlacementManagement.DTOs.Auth;

public class AuthResponseDto
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime TokenExpiry { get; set; }

    public List<string> Roles { get; set; } = new();
}
