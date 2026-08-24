using JobTrack.Api.Data;
using JobTrack.Api.DTOs.Companies;
using JobTrack.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompaniesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IEnumerable<CompanyResponse>),
        StatusCodes.Status200OK
    )]
    public async Task<ActionResult<IEnumerable<CompanyResponse>>>
        GetCompanies()
    {
        var companies = await _context.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Website = company.Website,
                Location = company.Location,
                CreatedAt = company.CreatedAt
            })
            .ToListAsync();

        return Ok(companies);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(CompanyResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponse>>
        GetCompany(int id)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .Where(company => company.Id == id)
            .Select(company => new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Website = company.Website,
                Location = company.Location,
                CreatedAt = company.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (company is null)
        {
            return NotFound(new
            {
                message = $"Company with ID {id} was not found."
            });
        }

        return Ok(company);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(CompanyResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<CompanyResponse>>
        UpdateCompany(int id, UpdateCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company is null)
        {
            return NotFound(new
            {
                message = $"Company with ID {id} was not found."
            });
        }

        var companyName = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            ModelState.AddModelError(
                nameof(request.Name),
                "Company name is required."
            );

            return ValidationProblem(ModelState);
        }

        var companyExists = await _context.Companies
            .AnyAsync(existingCompany =>
                existingCompany.Id != id
                && existingCompany.Name == companyName
            );

        if (companyExists)
        {
            return Conflict(new
            {
                message = "A company with this name already exists."
            });
        }

        company.Name = companyName;
        company.Website = string.IsNullOrWhiteSpace(request.Website)
            ? null
            : request.Website.Trim();
        company.Location = string.IsNullOrWhiteSpace(request.Location)
            ? null
            : request.Location.Trim();

        await _context.SaveChangesAsync();

        var response = new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            Website = company.Website,
            Location = company.Location,
            CreatedAt = company.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CompanyResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CompanyResponse>>
        CreateCompany(CreateCompanyRequest request)
    {
        var companyName = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(companyName))
        {
            ModelState.AddModelError(
                nameof(request.Name),
                "Company name is required."
            );

            return ValidationProblem(ModelState);
        }

        var companyExists = await _context.Companies
            .AnyAsync(company => company.Name == companyName);

        if (companyExists)
        {
            return Conflict(new
            {
                message = "A company with this name already exists."
            });
        }

        var company = new Company
        {
            Name = companyName,
            Website = string.IsNullOrWhiteSpace(request.Website)
                ? null
                : request.Website.Trim(),
            Location = string.IsNullOrWhiteSpace(request.Location)
                ? null
                : request.Location.Trim()
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        var response = new CompanyResponse
        {
            Id = company.Id,
            Name = company.Name,
            Website = company.Website,
            Location = company.Location,
            CreatedAt = company.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetCompany),
            new { id = company.Id },
            response
        );
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);

        if (company is null)
        {
            return NotFound(new
            {
                message = $"Company with ID {id} was not found."
            });
        }

        var hasJobApplications = await _context.JobApplications
            .AnyAsync(application => application.CompanyId == id);

        if (hasJobApplications)
        {
            return Conflict(new
            {
                message =
                    "Company cannot be deleted because it has job applications."
            });
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}