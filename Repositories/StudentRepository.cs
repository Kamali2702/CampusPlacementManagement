
using CampusPlacementManagement.Data;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly ApplicationDbContext _context;

    public StudentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int studentId)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public async Task<Student?> GetByUserIdAsync(string userId)
    {
        return await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<List<Student>> GetAllAsync(
        int? collegeId,
        string? search,
        string? department,
        int? graduationYear,
        StudentStatus? status,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Students
            .AsNoTracking()
            .AsQueryable();

        if (collegeId.HasValue)
        {
            query = query.Where(s =>
                s.CollegeId == collegeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.RegisterNumber.Contains(search) ||
                _context.Users.Any(u =>
                    u.Id == s.UserId &&
                    u.FullName.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(s =>
                s.Department == department);
        }

        if (graduationYear.HasValue)
        {
            query = query.Where(s =>
                s.GraduationYear == graduationYear.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        return await query
            .OrderBy(s => s.StudentId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Student>> GetPendingAsync(int collegeId)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s =>
                s.CollegeId == collegeId &&
                s.Status == StudentStatus.Pending)
            .OrderBy(s => s.StudentId)
            .ToListAsync();
    }

    public async Task<bool> RegisterNumberExistsAsync(
        int collegeId,
        string registerNumber)
    {
        return await _context.Students
            .AnyAsync(s =>
                s.CollegeId == collegeId &&
                s.RegisterNumber == registerNumber);
    }

    public async Task AddAsync(Student student)
    {
        await _context.Students.AddAsync(student);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Student student)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync();
    }
}
