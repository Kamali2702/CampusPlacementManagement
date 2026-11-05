
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _collegeRepository;

    public CollegeService(ICollegeRepository collegeRepository)
    {
        _collegeRepository = collegeRepository;
    }

    public async Task<CollegeResponseDto> RegisterCollegeAsync(
        RegisterCollegeRequestDto request)
    {
        string collegeCode = request.CollegeCode.Trim();

        bool codeExists =
            await _collegeRepository.CollegeCodeExistsAsync(collegeCode);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A college with this code already exists.");
        }

        var college = new College
        {
            CollegeName = request.CollegeName.Trim(),
            CollegeCode = collegeCode,
            UniversityName = request.UniversityName?.Trim(),
            OfficialEmail = request.OfficialEmail.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            Website = request.Website?.Trim(),
            Status = CollegeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _collegeRepository.AddAsync(college);
        await _collegeRepository.SaveChangesAsync();

        return MapToResponse(college);
    }

    public async Task<List<CollegeResponseDto>> GetAllCollegesAsync()
    {
        var colleges = await _collegeRepository.GetAllAsync();
        return colleges.Select(MapToResponse).ToList();
    }

    public async Task<CollegeResponseDto?> GetCollegeByIdAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        return college == null ? null : MapToResponse(college);
    }

    public async Task<List<CollegeResponseDto>> GetPendingCollegesAsync()
    {
        var colleges = await _collegeRepository.GetPendingAsync();
        return colleges.Select(MapToResponse).ToList();
    }

    public async Task<bool> ApproveCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
            return false;

        if (college.Status != CollegeStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending colleges can be approved.");
        }

        college.Status = CollegeStatus.Approved;
        await _collegeRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
            return false;

        if (college.Status != CollegeStatus.Pending)
        {
            throw new InvalidOperationException(
                "Only pending colleges can be rejected.");
        }

        college.Status = CollegeStatus.Rejected;
        await _collegeRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
            return false;

        if (college.Status != CollegeStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved colleges can be suspended.");
        }

        college.Status = CollegeStatus.Suspended;
        await _collegeRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateCollegeAsync(
        int collegeId,
        UpdateCollegeRequestDto request)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);

        if (college == null)
            return false;

        string collegeCode = request.CollegeCode.Trim();

        bool codeExists =
            await _collegeRepository.CollegeCodeExistsAsync(
                collegeCode, collegeId);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Another college already uses this code.");
        }

        college.CollegeName = request.CollegeName.Trim();
        college.CollegeCode = collegeCode;
        college.UniversityName = request.UniversityName?.Trim();
        college.OfficialEmail = request.OfficialEmail.Trim();
        college.PhoneNumber = request.PhoneNumber.Trim();
        college.Address = request.Address.Trim();
        college.Website = request.Website?.Trim();

        await _collegeRepository.SaveChangesAsync();

        return true;
    }

    private static CollegeResponseDto MapToResponse(College college)
    {
        return new CollegeResponseDto
        {
            CollegeId = college.CollegeId,
            CollegeName = college.CollegeName,
            CollegeCode = college.CollegeCode,
            UniversityName = college.UniversityName,
            OfficialEmail = college.OfficialEmail,
            PhoneNumber = college.PhoneNumber,
            Address = college.Address,
            Website = college.Website,
            Status = college.Status.ToString(),
            CreatedAt = college.CreatedAt
        };
    }
}
