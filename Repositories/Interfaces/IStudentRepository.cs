
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;

namespace CampusPlacementManagement.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int studentId);

    Task<Student?> GetByUserIdAsync(string userId);

    Task<List<Student>> GetAllAsync(
        int? collegeId,
        string? search,
        string? department,
        int? graduationYear,
        StudentStatus? status,
        int pageNumber,
        int pageSize);

    Task<List<Student>> GetPendingAsync(int collegeId);

    Task<bool> RegisterNumberExistsAsync(
        int collegeId,
        string registerNumber);

    Task AddAsync(Student student);

    Task UpdateAsync(Student student);
}
