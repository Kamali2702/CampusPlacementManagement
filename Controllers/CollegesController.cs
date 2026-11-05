using System.Security.Claims;
using CampusPlacementManagement.Data;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CollegesController : ControllerBase
{
    private readonly ICollegeService _collegeService;
    private readonly ApplicationDbContext _context;

    public CollegesController(
        ICollegeService collegeService,
        ApplicationDbContext context)
    {
        _collegeService = collegeService;
        _context = context;
    }

    // Public college registration
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<CollegeResponseDto>> RegisterCollege(
        RegisterCollegeRequestDto request)
    {
        try
        {
            var result =
                await _collegeService.RegisterCollegeAsync(request);

            return CreatedAtAction(
                nameof(GetCollegeById),
                new { id = result.CollegeId },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // SuperAdmin: View all colleges
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<CollegeResponseDto>>> GetAllColleges()
    {
        var colleges = await _collegeService.GetAllCollegesAsync();
        return Ok(colleges);
    }

    // SuperAdmin: View any college
    // PlacementOfficer: View only their associated college
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
    public async Task<ActionResult<CollegeResponseDto>> GetCollegeById(int id)
    {
        if (User.IsInRole("PlacementOfficer"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAssociated = await _context.PlacementOfficers
                .AnyAsync(p =>
                    p.UserId == userId &&
                    p.CollegeId == id);

            if (!isAssociated)
                return Forbid();
        }

        var college = await _collegeService.GetCollegeByIdAsync(id);

        if (college == null)
            return NotFound(new { message = "College not found." });

        return Ok(college);
    }

    // SuperAdmin: View pending colleges
    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<CollegeResponseDto>>> GetPendingColleges()
    {
        var colleges = await _collegeService.GetPendingCollegesAsync();
        return Ok(colleges);
    }

    // SuperAdmin: Approve college
    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveCollege(int id)
    {
        try
        {
            bool result = await _collegeService.ApproveCollegeAsync(id);

            if (!result)
                return NotFound(new { message = "College not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // SuperAdmin: Reject college
    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectCollege(int id)
    {
        try
        {
            bool result = await _collegeService.RejectCollegeAsync(id);

            if (!result)
                return NotFound(new { message = "College not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // SuperAdmin: Suspend college
    [HttpPut("{id:int}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SuspendCollege(int id)
    {
        try
        {
            bool result = await _collegeService.SuspendCollegeAsync(id);

            if (!result)
                return NotFound(new { message = "College not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // SuperAdmin: Update college
    [HttpPut("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateCollege(
        int id,
        UpdateCollegeRequestDto request)
    {
        try
        {
            bool result =
                await _collegeService.UpdateCollegeAsync(id, request);

            if (!result)
                return NotFound(new { message = "College not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}