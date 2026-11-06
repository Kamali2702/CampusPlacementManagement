
using CampusPlacementManagement.DTOs.CompanyHR;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagement.Services;

public class CompanyHRService : ICompanyHRService
{
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICompanyRepository _companyRepository;

    public CompanyHRService(
        ICompanyHRRepository companyHRRepository,
        UserManager<ApplicationUser> userManager,
        ICompanyRepository companyRepository)
    {
        _companyHRRepository = companyHRRepository;
        _userManager = userManager;
        _companyRepository = companyRepository;
    }

    public async Task<int> RegisterAsync(RegisterCompanyHRRequestDto request)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);

        if (company == null || company.Status != CompanyStatus.Approved)
        {
            throw new InvalidOperationException(
                "Company not found or is not approved.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AccountStatus = AccountStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                identityResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        var companyHR = new CompanyHR
        {
            UserId = user.Id,
            CompanyId = request.CompanyId,
            Designation = request.Designation?.Trim()
        };

        try
        {
            await _companyHRRepository.AddAsync(companyHR);
            await _companyHRRepository.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return companyHR.CompanyHRId;
    }

    public async Task<List<CompanyHRResponseDto>> GetAllAsync()
    {
        var companyHRs = await _companyHRRepository.GetAllAsync();
        var result = new List<CompanyHRResponseDto>();

        foreach (var companyHR in companyHRs)
        {
            var dto = await MapToDtoAsync(companyHR);

            if (dto != null)
                result.Add(dto);
        }

        return result;
    }

    public async Task<List<CompanyHRResponseDto>> GetPendingAsync()
    {
        var companyHRs = await _companyHRRepository.GetAllAsync();
        var result = new List<CompanyHRResponseDto>();

        foreach (var companyHR in companyHRs)
        {
            var dto = await MapToDtoAsync(companyHR);

            if (dto != null &&
                dto.AccountStatus == AccountStatus.Pending.ToString())
            {
                result.Add(dto);
            }
        }

        return result;
    }

    public async Task<CompanyHRResponseDto?> GetByIdAsync(int companyHRId)
    {
        var companyHR = await _companyHRRepository.GetByIdAsync(companyHRId);

        return companyHR == null
            ? null
            : await MapToDtoAsync(companyHR);
    }

    public async Task<CompanyHRResponseDto?> GetMyProfileAsync(string userId)
    {
        var companyHR = await _companyHRRepository.GetByUserIdAsync(userId);

        return companyHR == null
            ? null
            : await MapToDtoAsync(companyHR);
    }

    public async Task<bool> ApproveAsync(int companyHRId, string approvedBy)
    {
        var companyHR = await _companyHRRepository.GetByIdAsync(companyHRId);

        if (companyHR == null || companyHR.ApprovedAt != null)
            return false;

        var user = await _userManager.FindByIdAsync(companyHR.UserId);

        if (user == null || user.AccountStatus != AccountStatus.Pending)
            return false;

        user.AccountStatus = AccountStatus.Approved;
        user.UpdatedAt = DateTime.UtcNow;

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "CompanyHR");

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        companyHR.ApprovedBy = approvedBy;
        companyHR.ApprovedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, "CompanyHR");

            var errors = string.Join(
                "; ",
                updateResult.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        await _companyHRRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectAsync(int companyHRId)
    {
        var companyHR = await _companyHRRepository.GetByIdAsync(companyHRId);

        if (companyHR == null || companyHR.ApprovedAt != null)
            return false;

        var user = await _userManager.FindByIdAsync(companyHR.UserId);

        if (user == null || user.AccountStatus != AccountStatus.Pending)
            return false;

        user.AccountStatus = AccountStatus.Rejected;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }

        await _companyHRRepository.SaveChangesAsync();

        return true;
    }

    private async Task<CompanyHRResponseDto?> MapToDtoAsync(
        CompanyHR companyHR)
    {
        var user = await _userManager.FindByIdAsync(companyHR.UserId);

        if (user == null)
            return null;

        var company = await _companyRepository.GetByIdAsync(
            companyHR.CompanyId);

        if (company == null)
            return null;

        return new CompanyHRResponseDto
        {
            CompanyHRId = companyHR.CompanyHRId,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            Designation = companyHR.Designation,
            AccountStatus = user.AccountStatus.ToString(),
            ApprovedBy = companyHR.ApprovedBy,
            ApprovedAt = companyHR.ApprovedAt
        };
    }
}
