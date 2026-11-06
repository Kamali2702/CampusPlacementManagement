using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
    }

    public async Task<List<Company>> GetAllAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int companyId)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == companyId);
    }

    public async Task<List<Company>> GetPendingAsync()
    {
        return await _context.Companies
            .AsNoTracking()
            .Where(c => c.Status == Enums.CompanyStatus.Pending)
            .ToListAsync();
    }

    public async Task<bool> CompanyEmailExistsAsync(
        string companyEmail,
        int? excludeCompanyId = null)
    {
        return await _context.Companies.AnyAsync(c =>
            c.CompanyEmail == companyEmail &&
            (!excludeCompanyId.HasValue ||
             c.CompanyId != excludeCompanyId.Value));
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}