using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using JobTrack.Api.Data;
using JobTrack.Api.DTOs.Auth;
using JobTrack.Api.Models;
using JobTrack.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthController(
        AppDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        TokenService tokenService
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [ProducesResponseType(
        typeof(AuthResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<AuthResponse>>
        Register(RegisterRequest request)
    {
        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(firstName))
        {
            ModelState.AddModelError(
                nameof(request.FirstName),
                "First name is required."
            );
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            ModelState.AddModelError(
                nameof(request.LastName),
                "Last name is required."
            );
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var emailExists = await _context.Users
            .AnyAsync(user => user.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new AppUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(
    StatusCodes.Status201Created,
    CreateAuthResponse(user)
);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CurrentUserResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> GetCurrentUser()
    {
        var userIdValue = User
            .FindFirst(JwtRegisteredClaimNames.Sub)?
            .Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "The token does not contain a valid user ID."
            });
        }

        var response = new CurrentUserResponse
        {
            UserId = userId,
            FirstName = User
                .FindFirst(JwtRegisteredClaimNames.GivenName)?
                .Value ?? string.Empty,
            LastName = User
                .FindFirst(JwtRegisteredClaimNames.FamilyName)?
                .Value ?? string.Empty,
            Email = User
                .FindFirst(JwtRegisteredClaimNames.Email)?
                .Value ?? string.Empty
        };

        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AuthResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<AuthResponse>>
        Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password
            );

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password
            );

            await _context.SaveChangesAsync();
        }

        return Ok(CreateAuthResponse(user));
    }

    private AuthResponse CreateAuthResponse(AppUser user)
    {
        var (token, expiresAt) =
            _tokenService.CreateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token,
            ExpiresAt = expiresAt
        };
    }
}
