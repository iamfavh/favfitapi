using FavFitApi.Models;
using FavFitApi.Services;
using FavFitApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace FavFitApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FavFitdbContext _context;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<User> _passwordHasher;

    private static readonly Lazy<string> DummyHash =
        new(() => new PasswordHasher<User>().HashPassword(new User(), Guid.NewGuid().ToString()));

    public AuthController(FavFitdbContext context, TokenService tokenService, IConfiguration config, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _tokenService = tokenService;
        _config = config;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] AuthDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            _passwordHasher.VerifyHashedPassword(new User(), DummyHash.Value, request.Password);
            return Unauthorized("Not authorized to perform that request");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (verificationResult == PasswordVerificationResult.Failed)
            return Unauthorized("Not authorized to perform that request");

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var accessToken = _tokenService.GenerateAccessToken(BuildClaims(user));
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(_tokenService.GenerateRefreshToken()),
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var savedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId.Equals(user.Id));

        if (savedRefreshToken == null)
        {
            await _context.RefreshTokens.AddAsync(refreshToken);
        }
        else
        {
            savedRefreshToken.TokenHash = refreshToken.TokenHash;
            savedRefreshToken.IssuedAt = refreshToken.IssuedAt;
            savedRefreshToken.ExpiresAt = refreshToken.ExpiresAt;
        }
            
        await _context.SaveChangesAsync();

        return Ok( new 
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.TokenHash
            }
        );    
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh([FromBody] AuthRequestDto request)
    {   
        
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

        if (principal == null)
            return Unauthorized("Invalid access token");
        
        var savedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash.Equals(request.RefreshToken));

        var principalUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (savedRefreshToken == null
            || savedRefreshToken.ExpiresAt <= DateTime.UtcNow
            || savedRefreshToken.UserId.ToString() != principalUserId)
        {
            return Unauthorized("Invalid refresh token");
        }

        var user = await _context.Users.FindAsync(savedRefreshToken.UserId);

        if (user == null)
            return Unauthorized("Invalid refresh token");

        var newAccessToken = _tokenService.GenerateAccessToken(BuildClaims(user));
        savedRefreshToken.TokenHash = _tokenService.GenerateRefreshToken();
        savedRefreshToken.IssuedAt = DateTime.UtcNow;
        savedRefreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync();

        return Ok(new
        {
           AccessToken = newAccessToken,
           RefreshToken = savedRefreshToken.TokenHash
        });
        
    }

    private static List<Claim> BuildClaims(User user) =>
    [
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email)
    ];
}