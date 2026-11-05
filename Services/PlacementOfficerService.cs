using CampusPlacementManagement.Data;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Services;

public class PlacementOfficerService : IPlacementOfficerService
{
    private readonly IPlacementOfficerRepository _repository;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlacementOfficerService(
        IPlacementOfficerRepository repository,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _repository = repository;
        _context = context;
        _userManager = userManager;
    }

    public async Task<int> RegisterAsync(
        RegisterPlacementOfficerRequestDto request)
    {
        var college = await _context.Colleges
            .FirstOrDefaultAsync(c => c.CollegeId == request.CollegeId);

        if (college == null)
            throw new InvalidOperationException("College not found.");

        if (college.Status != CollegeStatus.Approved)
            throw new InvalidOperationException(
                "Placement officers can register only with approved colleges.");

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
            throw new InvalidOperationException(
                "An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            AccountStatus = AccountStatus.Pending
        };

        var identityResult = await _userManager.CreateAsync(
            user, request.Password);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join(
                "; ", identityResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        var placementOfficer = new PlacementOfficer
        {
            UserId = user.Id,
            CollegeId = request.CollegeId,
            Designation = request.Designation,
            Status = PlacementOfficerStatus.Pending
        };

        await _repository.AddAsync(placementOfficer);
        await _repository.SaveChangesAsync();

        return placementOfficer.PlacementOfficerId;
    }

    public async Task<List<PlacementOfficerResponseDto>> GetAllAsync()
    {
        var officers = await _repository.GetAllAsync();
        var result = new List<PlacementOfficerResponseDto>();

        foreach (var officer in officers)
        {
            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user != null)
                result.Add(MapToDto(officer, user));
        }

        return result;
    }

    public async Task<List<PlacementOfficerResponseDto>> GetPendingAsync()
    {
        var officers = await _repository.GetPendingAsync();
        var result = new List<PlacementOfficerResponseDto>();

        foreach (var officer in officers)
        {
            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user != null)
                result.Add(MapToDto(officer, user));
        }

        return result;
    }

    public async Task<PlacementOfficerResponseDto?> GetByIdAsync(int id)
    {
        var officer = await _repository.GetByIdAsync(id);

        if (officer == null)
            return null;

        var user = await _userManager.FindByIdAsync(officer.UserId);

        return user == null ? null : MapToDto(officer, user);
    }

    public async Task<PlacementOfficerResponseDto?> GetMyProfileAsync(
        string userId)
    {
        var officer = await _repository.GetByUserIdAsync(userId);

        if (officer == null)
            return null;

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return null;

        return MapToDto(officer, user);
    }

    public async Task<bool> ApproveAsync(int id, string approvedBy)
    {
        var officer = await _repository.GetByIdAsync(id);

        if (officer == null)
            return false;

        if (officer.Status != PlacementOfficerStatus.Pending)
            throw new InvalidOperationException(
                "Only pending placement officers can be approved.");

        var college = await _context.Colleges
            .FirstOrDefaultAsync(c => c.CollegeId == officer.CollegeId);

        if (college == null || college.Status != CollegeStatus.Approved)
            throw new InvalidOperationException(
                "The associated college is not approved.");

        var user = await _userManager.FindByIdAsync(officer.UserId);

        if (user == null)
            throw new InvalidOperationException(
                "Placement officer account not found.");

        user.AccountStatus = AccountStatus.Approved;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            throw new InvalidOperationException(
                "Unable to approve the user account.");

        if (!await _userManager.IsInRoleAsync(user, "PlacementOfficer"))
        {
            var roleResult = await _userManager.AddToRoleAsync(
                user, "PlacementOfficer");

            if (!roleResult.Succeeded)
                throw new InvalidOperationException(
                    "Unable to assign PlacementOfficer role.");
        }

        officer.Status = PlacementOfficerStatus.Approved;
        officer.ApprovedBy = approvedBy;
        officer.ApprovedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectAsync(int id)
    {
        var officer = await _repository.GetByIdAsync(id);

        if (officer == null)
            return false;

        if (officer.Status != PlacementOfficerStatus.Pending)
            throw new InvalidOperationException(
                "Only pending placement officers can be rejected.");

        var user = await _userManager.FindByIdAsync(officer.UserId);

        if (user == null)
            throw new InvalidOperationException(
                "Placement officer account not found.");

        user.AccountStatus = AccountStatus.Rejected;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Unable to reject the user account.");

        officer.Status = PlacementOfficerStatus.Rejected;

        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendAsync(int id)
    {
        var officer = await _repository.GetByIdAsync(id);

        if (officer == null)
            return false;

        if (officer.Status != PlacementOfficerStatus.Approved)
            throw new InvalidOperationException(
                "Only approved placement officers can be suspended.");

        var user = await _userManager.FindByIdAsync(officer.UserId);

        if (user == null)
            throw new InvalidOperationException(
                "Placement officer account not found.");

        user.AccountStatus = AccountStatus.Suspended;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new InvalidOperationException(
                "Unable to suspend the user account.");

        officer.Status = PlacementOfficerStatus.Suspended;

        await _repository.SaveChangesAsync();

        return true;
    }

    private static PlacementOfficerResponseDto MapToDto(
        PlacementOfficer officer,
        ApplicationUser user)
    {
        return new PlacementOfficerResponseDto
        {
            PlacementOfficerId = officer.PlacementOfficerId,
            UserId = officer.UserId,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CollegeId = officer.CollegeId,
            Designation = officer.Designation,
            Status = officer.Status.ToString(),
            ApprovedBy = officer.ApprovedBy,
            ApprovedAt = officer.ApprovedAt
        };
    }
}