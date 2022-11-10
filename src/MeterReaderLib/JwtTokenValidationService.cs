namespace MeterReaderLib;

using MeterReaderLib.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
    
public class JwtTokenValidationService
{
    private readonly UserManager<IdentityUser> userManager;
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly IConfiguration configuration;

    public JwtTokenValidationService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
    }

    public async Task<TokenModel> GenerateTokenModelAsync(CredentialModel model)
    {
        var user = await this.userManager.FindByNameAsync(model.UserName);
        var result = new TokenModel
        {
            Success = false
        };

        if (user is null)
        {
            return result;
        }

        var check = await this.signInManager.CheckPasswordSignInAsync(user, model.Passcode, false);

        if (!check.Succeeded)
        {
            return result;
        }

        // Create the token
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Tokens:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            this.configuration["Tokens:Issuer"],
            this.configuration["Tokens:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        result.Token = new JwtSecurityTokenHandler().WriteToken(token);
        result.Expiration = token.ValidTo;
        result.Success = true;

        return result;
    }
}