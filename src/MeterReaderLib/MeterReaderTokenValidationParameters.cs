namespace MeterReaderLib;

using Microsoft.IdentityModel.Tokens;
using System.Text;

public class MeterReaderTokenValidationParameters : TokenValidationParameters
{
    public MeterReaderTokenValidationParameters(IConfiguration config)
    {
        this.ValidIssuer = config["Tokens:Issuer"];
        this.ValidAudience = config["Tokens:Audience"];
        this.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Tokens:Key"]));
    }
}