using System.Security.Claims;
using CampusPlacementManagement.DTOs;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[ApiController]
[Route("api/placement-officers")]
[Authorize]
public class PlacementOfficersController : ControllerBase
{
    private readonly IPlacementOfficerService _service;

    public PlacementOfficersController(IPlacementOfficerService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterPlacementOfficerRequestDto request)
    {
        try
        {
            var id = await _service.RegisterAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id },
                new
                {
                    placementOfficerId = id,
                    message = "Registration submitted successfully."
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPending()
    {
        return Ok(await _service.GetPendingAsync());
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
    public async Task<IActionResult> GetById(int id)
    {
        var officer = await _service.GetByIdAsync(id);

        if (officer == null)
            return NotFound(new { message = "Placement officer not found." });

        if (User.IsInRole("PlacementOfficer"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (officer.UserId != userId)
                return Forbid();
        }

        return Ok(officer);
    }

    [HttpGet("me")]
    [Authorize(Roles = "PlacementOfficer")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _service.GetMyProfileAsync(userId);

        if (profile == null)
            return NotFound(new { message = "Placement officer profile not found." });

        return Ok(profile);
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var approvedBy = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _service.ApproveAsync(id, approvedBy);

            if (!result)
                return NotFound(new { message = "Placement officer not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var result = await _service.RejectAsync(id);

            if (!result)
                return NotFound(new { message = "Placement officer not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/suspend")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Suspend(int id)
    {
        try
        {
            var result = await _service.SuspendAsync(id);

            if (!result)
                return NotFound(new { message = "Placement officer not found." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}