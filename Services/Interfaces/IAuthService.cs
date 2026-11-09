
using CampusPlacementManagement.DTOs.Auth;

namespace CampusPlacementManagement.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);

    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<string> GenerateStudentPasswordSetupTokenAsync(string email);

    Task<bool> SetStudentPasswordAsync(SetStudentPasswordRequestDto request);
}

