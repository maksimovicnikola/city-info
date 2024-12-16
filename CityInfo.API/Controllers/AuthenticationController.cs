using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CityInfo.API.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public class AuthenticationRequestBody
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public AuthenticationController(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private class CityInfoUser
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }

        public CityInfoUser(int userId, string username, string firstName, string lastName, string city)
        {
            UserId = userId;
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            City = city;
        }
    }

    [HttpPost("authenticate")]
    public IActionResult Authenticate(AuthenticationRequestBody requestBody)
    {
        // Step 1: Validate username and password
        var user = ValidateUserCredentials(requestBody.Username, requestBody.Password);

        if (user == null)
        {
            return Unauthorized();
        }

        // Step 2: Create a token
        // NuGet Package: Microsoft.IdentityModel.Tokens
        var securityKey =
            new SymmetricSecurityKey(Convert.FromBase64String(_configuration["Authentication:SecretForKey"]));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsForToken = new List<Claim>();
        claimsForToken.Add(new Claim("sub", user.UserId.ToString()));
        claimsForToken.Add(new Claim("given_name", user.FirstName));
        claimsForToken.Add(new Claim("family_name", user.LastName));
        claimsForToken.Add(new Claim("city", user.City));

        // NuGet Package: System.IdentityModel.Tokens.Jwt
        var jwtSecurityToken = new JwtSecurityToken(
            _configuration["Authentication:Issuer"],
            _configuration["Authentication:Audience"],
            claimsForToken,
            DateTime.UtcNow, // When the token is generated
            DateTime.Now.AddHours(1), // Indicates the end of token validity
            signingCredentials
        );
        
        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        
        return Ok(tokenToReturn);
    }

    private CityInfoUser ValidateUserCredentials(string? username, string? password)
    {
        // We don't have a user DB or table. If we had it, we should check the passed-through username/password agains what's stored in the database
        // For demo purposes, we assume the credentials are valid

        // Return a new CityInfoUser(values would normally come from user DB/table)
        return new CityInfoUser(
            1, username ?? "", "Nikola", "Maksimovic", "Novi Sad");
    }
}