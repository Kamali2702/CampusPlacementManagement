
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories;

public class CollegeRepository : ICollegeRepository
{
    private readonly ApplicationDbContext _context;

    public CollegeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(College college)
    {
        await _context.Colleges.AddAsync(college);
    }

    public async Task<List<College>> GetAllAsync()
    {
        return await _context.Colleges
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<College?> GetByIdAsync(int collegeId)
    {
        return await _context.Colleges
            .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
    }

    public async Task<List<College>> GetPendingAsync()
    {
        return await _context.Colleges
            .AsNoTracking()
            .Where(c => c.Status == CollegeStatus.Pending)
            .ToListAsync();
    }

    public async Task<bool> CollegeCodeExistsAsync(
        string collegeCode,
        int? excludeCollegeId = null)
    {
        var query = _context.Colleges
            .Where(c => c.CollegeCode == collegeCode);

        if (excludeCollegeId.HasValue)
        {
            query = query.Where(
                c => c.CollegeId != excludeCollegeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
