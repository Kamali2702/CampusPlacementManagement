using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Models;
using CampusPlacementManagement.Repositories.Interfaces;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace CampusPlacementManagement.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _placementOfficerRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentService(
        IStudentRepository studentRepository,
        IPlacementOfficerRepository placementOfficerRepository,
        ICollegeRepository collegeRepository,
        UserManager<ApplicationUser> userManager)
    {
        _studentRepository = studentRepository;
        _placementOfficerRepository = placementOfficerRepository;
        _collegeRepository = collegeRepository;
        _userManager = userManager;
    }

    // Individual Student Enrollment
    public async Task<StudentResponseDto> EnrollAsync(
        EnrollStudentRequestDto request,
        string placementOfficerUserId)
    {
        var officer = await _placementOfficerRepository
            .GetByUserIdAsync(placementOfficerUserId);

        if (officer == null ||
            officer.Status != PlacementOfficerStatus.Approved)
        {
            throw new UnauthorizedAccessException(
                "Only approved Placement Officers can enroll students.");
        }

        var college = await _collegeRepository.GetByIdAsync(
            officer.CollegeId);

        if (college == null || college.Status != CollegeStatus.Approved)
        {
            throw new InvalidOperationException(
                "The officer's college is not approved.");
        }

        var email = request.Email.Trim();

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }

        var registerNumber = request.RegisterNumber.Trim();

        if (await _studentRepository.RegisterNumberExistsAsync(
            officer.CollegeId,
            registerNumber))
        {
            throw new InvalidOperationException(
                "This register number already exists in the college.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            PhoneNumber = request.MobileNumber.Trim(),
            AccountStatus = AccountStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var identityResult = await _userManager.CreateAsync(user);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                identityResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException(errors);
        }

        var student = new Student
        {
            UserId = user.Id,
            CollegeId = officer.CollegeId,
            PlacementOfficerId = officer.PlacementOfficerId,
            RegisterNumber = registerNumber,
            Department = request.Department.Trim(),
            Degree = request.Degree.Trim(),
            GraduationYear = request.GraduationYear,
            CGPA = request.CGPA,
            ActiveBacklogs = request.ActiveBacklogs,
            ResumeUrl = request.ResumeUrl?.Trim(),
            Status = StudentStatus.Pending
        };

        try
        {
            await _studentRepository.AddAsync(student);
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        var response = await MapToDtoAsync(student);

        if (response == null)
        {
            throw new InvalidOperationException(
                "Unable to retrieve the newly enrolled student.");
        }

        return response;
    }

    // Bulk Student Enrollment using Excel
    public async Task<BulkStudentEnrollmentResponseDto> BulkEnrollAsync(
        IFormFile file,
        string placementOfficerUserId)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException(
                "Please upload a non-empty Excel file.");
        }

        if (!Path.GetExtension(file.FileName)
            .Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only .xlsx Excel files are supported.");
        }

        // Verify officer and college before processing rows.
        await GetApprovedOfficerAsync(placementOfficerUserId);

        using var stream = new MemoryStream();

        await file.CopyToAsync(stream);
        stream.Position = 0;

        XLWorkbook workbook;

        try
        {
            workbook = new XLWorkbook(stream);
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new ArgumentException(
                "The uploaded file is not a valid .xlsx workbook.");
        }

        using (workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                throw new ArgumentException(
                    "The Excel file does not contain a worksheet.");
            }

            // Expected columns:
            // A FullName, B Email, C MobileNumber, D RegisterNumber,
            // E Department, F Degree, G GraduationYear, H CGPA,
            // I ActiveBacklogs, J ResumeUrl.

            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

            if (lastRow < 2)
            {
                throw new ArgumentException(
                    "The Excel file contains no student rows.");
            }

            var response = new BulkStudentEnrollmentResponseDto();

            for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
            {
                if (Enumerable.Range(1, 10).All(
                    column => worksheet.Cell(rowNumber, column).IsEmpty()))
                {
                    continue;
                }

                response.TotalRows++;

                var registerNumber = worksheet
                    .Cell(rowNumber, 4).GetString().Trim();

                try
                {
                    if (!int.TryParse(
                        worksheet.Cell(rowNumber, 7).GetString(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var graduationYear))
                    {
                        throw new ArgumentException(
                            "GraduationYear must be a valid number.");
                    }

                    if (!decimal.TryParse(
                        worksheet.Cell(rowNumber, 8).GetString(),
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var cgpa))
                    {
                        throw new ArgumentException(
                            "CGPA must be a valid number.");
                    }

                    if (!int.TryParse(
                        worksheet.Cell(rowNumber, 9).GetString(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var activeBacklogs))
                    {
                        throw new ArgumentException(
                            "ActiveBacklogs must be a valid number.");
                    }

                    var request = new EnrollStudentRequestDto
                    {
                        FullName = worksheet.Cell(rowNumber, 1)
                            .GetString().Trim(),

                        Email = worksheet.Cell(rowNumber, 2)
                            .GetString().Trim(),

                        MobileNumber = worksheet.Cell(rowNumber, 3)
                            .GetString().Trim(),

                        RegisterNumber = registerNumber,

                        Department = worksheet.Cell(rowNumber, 5)
                            .GetString().Trim(),

                        Degree = worksheet.Cell(rowNumber, 6)
                            .GetString().Trim(),

                        GraduationYear = graduationYear,
                        CGPA = cgpa,
                        ActiveBacklogs = activeBacklogs,

                        ResumeUrl = worksheet.Cell(rowNumber, 10)
                            .GetString().Trim()
                    };

                    var validationResults = new List<ValidationResult>();

                    var validationContext = new ValidationContext(request);

                    if (!Validator.TryValidateObject(
                        request,
                        validationContext,
                        validationResults,
                        true))
                    {
                        throw new ArgumentException(
                            string.Join(
                                "; ",
                                validationResults.Select(
                                    result => result.ErrorMessage)));
                    }

                    await EnrollAsync(request, placementOfficerUserId);

                    response.SuccessfulEnrollments++;

                    response.Results.Add(
                        new BulkStudentEnrollmentRowResultDto
                        {
                            RowNumber = rowNumber,
                            RegisterNumber = registerNumber,
                            Status = "Success",
                            Message =
                                "Student enrolled successfully with Pending status."
                        });
                }
                catch (Exception ex) when (
                    ex is ArgumentException ||
                    ex is InvalidOperationException)
                {
                    response.FailedEnrollments++;

                    response.Results.Add(
                        new BulkStudentEnrollmentRowResultDto
                        {
                            RowNumber = rowNumber,
                            RegisterNumber = registerNumber,
                            Status = "Failed",
                            Message = ex.Message
                        });
                }
            }

            return response;
        }
    }

    // Get All Students with Filtering and Pagination
    public async Task<List<StudentResponseDto>> GetAllAsync(
        string userId,
        bool isSuperAdmin,
        string? search,
        string? department,
        int? graduationYear,
        StudentStatus? status,
        int pageNumber,
        int pageSize)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException(
                "Page number must be positive and page size must be between 1 and 100.");
        }

        int? collegeId = null;

        if (!isSuperAdmin)
        {
            var officer = await GetApprovedOfficerAsync(userId);
            collegeId = officer.CollegeId;
        }

        var students = await _studentRepository.GetAllAsync(
            collegeId,
            search,
            department,
            graduationYear,
            status,
            pageNumber,
            pageSize);

        var result = new List<StudentResponseDto>();

        foreach (var student in students)
        {
            var response = await MapToDtoAsync(student);

            if (response != null)
            {
                result.Add(response);
            }
        }

        return result;
    }

    // Get Pending Students
    public async Task<List<StudentResponseDto>> GetPendingAsync(
        string placementOfficerUserId)
    {
        var officer = await GetApprovedOfficerAsync(
            placementOfficerUserId);

        var students = await _studentRepository.GetPendingAsync(
            officer.CollegeId);

        var result = new List<StudentResponseDto>();

        foreach (var student in students)
        {
            var response = await MapToDtoAsync(student);

            if (response != null)
            {
                result.Add(response);
            }
        }

        return result;
    }

    // Get Student by ID
    public async Task<StudentResponseDto?> GetByIdAsync(
     int studentId,
     string userId,
     bool isSuperAdmin,
     bool isStudent)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null)
            return null;

        if (isSuperAdmin)
            return await MapToDtoAsync(student);

        if (isStudent)
        {
            if (student.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only view your own student profile.");
            }

            await EnsureStudentCollegeApprovedAsync(student);

            return await MapToDtoAsync(student);
        }

        var officer = await GetApprovedOfficerAsync(userId);

        if (student.CollegeId != officer.CollegeId)
        {
            throw new UnauthorizedAccessException(
                "You cannot access students from another college.");
        }

        return await MapToDtoAsync(student);
    }
    // Approve Student
    public async Task<bool> ApproveAsync(
        int studentId,
        string placementOfficerUserId)
    {
        var officer = await GetApprovedOfficerAsync(
            placementOfficerUserId);

        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null ||
            student.CollegeId != officer.CollegeId ||
            student.Status != StudentStatus.Pending)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(student.UserId);

        if (user == null || user.AccountStatus != AccountStatus.Pending)
            return false;

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Student");

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description)));
        }

        user.AccountStatus = AccountStatus.Approved;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, "Student");

            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    updateResult.Errors.Select(e => e.Description)));
        }

        student.Status = StudentStatus.Approved;
        student.ApprovedAt = DateTime.UtcNow;

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    // Reject Student
    public async Task<bool> RejectAsync(
        int studentId,
        string placementOfficerUserId)
    {
        var officer = await GetApprovedOfficerAsync(
            placementOfficerUserId);

        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null ||
            student.CollegeId != officer.CollegeId ||
            student.Status != StudentStatus.Pending)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(student.UserId);

        if (user == null || user.AccountStatus != AccountStatus.Pending)
            return false;

        user.AccountStatus = AccountStatus.Rejected;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description)));
        }

        student.Status = StudentStatus.Rejected;

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    // Suspend Student
    public async Task<bool> SuspendAsync(
        int studentId,
        string userId,
        bool isSuperAdmin)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null)
            return false;

        if (!isSuperAdmin)
        {
            var officer = await GetApprovedOfficerAsync(userId);

            if (student.CollegeId != officer.CollegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot suspend students from another college.");
            }
        }

        var user = await _userManager.FindByIdAsync(student.UserId);

        if (user == null)
            return false;

        user.AccountStatus = AccountStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(e => e.Description)));
        }

        student.Status = StudentStatus.Suspended;

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    // Update Student
    public async Task<bool> UpdateAsync(
     int studentId,
     string userId,
     bool isSuperAdmin,
     bool isStudent,
     UpdateStudentRequestDto request)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);

        if (student == null)
            return false;

        if (isStudent)
        {
            if (student.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only update your own profile.");
            }

            await EnsureStudentCollegeApprovedAsync(student);
        }
        else if (!isSuperAdmin)
        {
            var officer = await GetApprovedOfficerAsync(userId);

            if (student.CollegeId != officer.CollegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update students from another college.");
            }
        }

        student.Department = request.Department.Trim();
        student.Degree = request.Degree.Trim();
        student.GraduationYear = request.GraduationYear;
        student.CGPA = request.CGPA;
        student.ActiveBacklogs = request.ActiveBacklogs;
        student.ResumeUrl = request.ResumeUrl?.Trim();

        await _studentRepository.UpdateAsync(student);

        return true;
    }

    // Get Logged-in Student Profile
    public async Task<StudentResponseDto?> GetMyProfileAsync(string userId)
    {
        var student = await _studentRepository.GetByUserIdAsync(userId);

        if (student == null)
            return null;

        await EnsureStudentCollegeApprovedAsync(student);

        return await MapToDtoAsync(student);
    }

    // Verify Approved Placement Officer and College
    private async Task<PlacementOfficer> GetApprovedOfficerAsync(
        string userId)
    {
        var officer = await _placementOfficerRepository
            .GetByUserIdAsync(userId);

        if (officer == null ||
            officer.Status != PlacementOfficerStatus.Approved)
        {
            throw new UnauthorizedAccessException(
                "An approved Placement Officer account is required.");
        }

        var college = await _collegeRepository.GetByIdAsync(
            officer.CollegeId);

        if (college == null || college.Status != CollegeStatus.Approved)
        {
            throw new UnauthorizedAccessException(
                "Your college is not currently approved.");
        }

        return officer;
    }

    // Verify that the student's college is approved
    private async Task EnsureStudentCollegeApprovedAsync(Student student)
    {
        var college = await _collegeRepository.GetByIdAsync(student.CollegeId);

        if (college == null || college.Status != CollegeStatus.Approved)
        {
            throw new UnauthorizedAccessException(
                "Student access is not allowed because the college is not approved.");
        }
    }

    // Map Student Entity to Response DTO
    private async Task<StudentResponseDto?> MapToDtoAsync(
        Student student)
    {
        var user = await _userManager.FindByIdAsync(student.UserId);

        if (user == null)
            return null;

        return new StudentResponseDto
        {
            StudentId = student.StudentId,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            MobileNumber = user.PhoneNumber ?? string.Empty,
            CollegeId = student.CollegeId,
            PlacementOfficerId = student.PlacementOfficerId,
            RegisterNumber = student.RegisterNumber,
            Department = student.Department,
            Degree = student.Degree,
            GraduationYear = student.GraduationYear,
            CGPA = student.CGPA,
            ActiveBacklogs = student.ActiveBacklogs,
            ResumeUrl = student.ResumeUrl,
            Status = student.Status,
            ApprovedAt = student.ApprovedAt
        };
    }
}