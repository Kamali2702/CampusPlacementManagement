using CampusPlacementManagement.DTOs.Company;
using CampusPlacementManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagement.Controllers;

[Route("api/companies")]
[ApiController]
[Authorize(Roles = "SuperAdmin")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    // Public company registration
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        RegisterCompanyRequestDto request)
    {
        try
        {
            int companyId = await _companyService.RegisterAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { companyId },
                new
                {
                    companyId,
                    message = "Company registration submitted successfully."
                });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Get all companies
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companies = await _companyService.GetAllAsync();

        return Ok(companies);
    }

    // Get pending companies
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var companies = await _companyService.GetPendingAsync();

        return Ok(companies);
    }

    // Get company by ID
    [HttpGet("{companyId:int}")]
    public async Task<IActionResult> GetById(int companyId)
    {
        var company = await _companyService.GetByIdAsync(companyId);

        if (company == null)
            return NotFound(new { message = "Company not found." });

        return Ok(company);
    }

    // Approve company
    [HttpPut("{companyId:int}/approve")]
    public async Task<IActionResult> Approve(int companyId)
    {
        bool result = await _companyService.ApproveAsync(companyId);

        if (!result)
            return BadRequest(new
            {
                message = "Company not found or is not in Pending status."
            });

        return Ok(new { message = "Company approved successfully." });
    }

    // Reject company
    [HttpPut("{companyId:int}/reject")]
    public async Task<IActionResult> Reject(int companyId)
    {
        bool result = await _companyService.RejectAsync(companyId);

        if (!result)
            return BadRequest(new
            {
                message = "Company not found or is not in Pending status."
            });

        return Ok(new { message = "Company rejected successfully." });
    }

    // Suspend company
    [HttpPut("{companyId:int}/suspend")]
    public async Task<IActionResult> Suspend(int companyId)
    {
        bool result = await _companyService.SuspendAsync(companyId);

        if (!result)
            return BadRequest(new
            {
                message = "Company not found or is not in Approved status."
            });

        return Ok(new { message = "Company suspended successfully." });
    }
}