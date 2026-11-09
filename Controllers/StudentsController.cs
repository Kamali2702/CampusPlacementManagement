
using System.Security.Claims;
using CampusPlacementManagement.DTOs.Student;
using CampusPlacementManagement.Enums;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[Route("api/students")]
[ApiController]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    private string? GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    [HttpPost("enroll")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<IActionResult> Enroll(
        EnrollStudentRequestDto request)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var student = await _studentService.EnrollAsync(
                request, userId);

            return Ok(new
            {
                message = "Student enrolled successfully. Awaiting approval.",
                student
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Bulk Student Enrollment using Excel
    [HttpPost("bulk-enroll")]
    [Authorize(Roles = "PlacementOfficer")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> BulkEnroll(
        [FromForm] IFormFile file)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.BulkEnrollAsync(
                file, userId);

            return Ok(new
            {
                message = "Bulk student enrollment processing completed.",
                result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] int? graduationYear,
        [FromQuery] StudentStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var students = await _studentService.GetAllAsync(
                userId,
                User.IsInRole("SuperAdmin"),
                search,
                department,
                graduationYear,
                status,
                pageNumber,
                pageSize);

            return Ok(students);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var students = await _studentService.GetPendingAsync(userId);

            return Ok(students);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var student = await _studentService.GetByIdAsync(
                id,
                userId,
                User.IsInRole("SuperAdmin"),
                User.IsInRole("Student"));

            if (student == null)
                return NotFound(new { message = "Student not found." });

            return Ok(student);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.ApproveAsync(id, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Student not found, outside your college, or not Pending."
                });

            return Ok(new
            {
                message = "Student approved successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.RejectAsync(id, userId);

            if (!result)
                return BadRequest(new
                {
                    message = "Student not found, outside your college, or not Pending."
                });

            return Ok(new
            {
                message = "Student rejected successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/suspend")]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
    public async Task<IActionResult> Suspend(int id)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.SuspendAsync(
                id,
                userId,
                User.IsInRole("SuperAdmin"));

            if (!result)
                return NotFound(new { message = "Student not found." });

            return Ok(new
            {
                message = "Student suspended successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
    public async Task<IActionResult> Update(
        int id,
        UpdateStudentRequestDto request)
    {
        try
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _studentService.UpdateAsync(
                id,
                userId,
                User.IsInRole("SuperAdmin"),
                User.IsInRole("Student"),
                request);

            if (!result)
                return NotFound(new { message = "Student not found." });

            return Ok(new
            {
                message = "Student details updated successfully."
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetUserId();

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var student = await _studentService.GetMyProfileAsync(userId);

        if (student == null)
            return NotFound(new { message = "Student profile not found." });

        return Ok(student);
    }
}
