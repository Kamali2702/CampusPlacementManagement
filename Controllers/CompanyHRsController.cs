using System.Security.Claims;
using CampusPlacementManagement.DTOs.CompanyHR;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[Route("api/company-hrs")]
[ApiController]
[Authorize]
public class CompanyHRsController : ControllerBase
{
    private readonly ICompanyHRService _companyHRService;

    public CompanyHRsController(ICompanyHRService companyHRService)
    {
        _companyHRService = companyHRService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterCompanyHRRequestDto request)
    {
        try
        {
            int companyHRId =
                await _companyHRService.RegisterAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = companyHRId },
                new
                {
                    companyHRId,
                    message = "Company HR registration submitted successfully."
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
        var companyHRs = await _companyHRService.GetAllAsync();
        return Ok(companyHRs);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetPending()
    {
        var companyHRs = await _companyHRService.GetPendingAsync();
        return Ok(companyHRs);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var companyHR = await _companyHRService.GetByIdAsync(id);

        if (companyHR == null)
            return NotFound(new { message = "Company HR not found." });

        return Ok(companyHR);
    }

    [HttpGet("me")]
    [Authorize(Roles = "CompanyHR")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var companyHR =
            await _companyHRService.GetMyProfileAsync(userId);

        if (companyHR == null)
            return NotFound(new { message = "Company HR profile not found." });

        return Ok(companyHR);
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            bool result = await _companyHRService.ApproveAsync(
                id,
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (!result)
                return BadRequest(new
                {
                    message = "Company HR not found or is not Pending."
                });

            return Ok(new
            {
                message = "Company HR approved successfully."
            });
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
            bool result = await _companyHRService.RejectAsync(id);

            if (!result)
                return BadRequest(new
                {
                    message = "Company HR not found or is not Pending."
                });

            return Ok(new
            {
                message = "Company HR rejected successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}