using CampusPlacementManagement.DTOs.Company;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;

namespace CampusPlacementManagement.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<int> RegisterAsync(RegisterCompanyRequestDto request)
    {
        bool emailExists = await _companyRepository
            .CompanyEmailExistsAsync(request.CompanyEmail);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A company with this email already exists.");
        }

        var company = new Company
        {
            CompanyName = request.CompanyName.Trim(),
            CompanyEmail = request.CompanyEmail.Trim(),
            Website = request.Website?.Trim(),
            Industry = request.Industry.Trim(),
            Address = request.Address.Trim(),
            Description = request.Description?.Trim(),
            Status = CompanyStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _companyRepository.AddAsync(company);
        await _companyRepository.SaveChangesAsync();

        return company.CompanyId;
    }

    public async Task<List<CompanyResponseDto>> GetAllAsync()
    {
        var companies = await _companyRepository.GetAllAsync();

        return companies.Select(MapToDto).ToList();
    }

    public async Task<List<CompanyResponseDto>> GetPendingAsync()
    {
        var companies = await _companyRepository.GetPendingAsync();

        return companies.Select(MapToDto).ToList();
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);

        return company == null ? null : MapToDto(company);
    }

    public async Task<bool> ApproveAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);

        if (company == null || company.Status != CompanyStatus.Pending)
            return false;

        company.Status = CompanyStatus.Approved;

        await _companyRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);

        if (company == null || company.Status != CompanyStatus.Pending)
            return false;

        company.Status = CompanyStatus.Rejected;

        await _companyRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);

        if (company == null || company.Status != CompanyStatus.Approved)
            return false;

        company.Status = CompanyStatus.Suspended;

        await _companyRepository.SaveChangesAsync();

        return true;
    }

    private static CompanyResponseDto MapToDto(Company company)
    {
        return new CompanyResponseDto
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            CompanyEmail = company.CompanyEmail,
            Website = company.Website,
            Industry = company.Industry,
            Address = company.Address,
            Description = company.Description,
            Status = company.Status.ToString(),
            CreatedAt = company.CreatedAt
        };
    }
}