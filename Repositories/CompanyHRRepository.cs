using CampusPlacementManagement.Data;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories;

public class CompanyHRRepository : ICompanyHRRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyHRRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CompanyHR companyHR)
    {
        await _context.CompanyHRs.AddAsync(companyHR);
    }

    public async Task<List<CompanyHR>> GetAllAsync()
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CompanyHR>> GetPendingAsync()
    {
        return await _context.CompanyHRs
            .AsNoTracking()
            .Where(hr => hr.ApprovedAt == null)
            .ToListAsync();
    }

    public async Task<CompanyHR?> GetByIdAsync(int companyHRId)
    {
        return await _context.CompanyHRs
            .FirstOrDefaultAsync(hr => hr.CompanyHRId == companyHRId);
    }

    public async Task<CompanyHR?> GetByUserIdAsync(string userId)
    {
        return await _context.CompanyHRs
            .FirstOrDefaultAsync(hr => hr.UserId == userId);
    }

    public async Task<bool> UserIdExistsAsync(string userId)
    {
        return await _context.CompanyHRs
            .AnyAsync(hr => hr.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}