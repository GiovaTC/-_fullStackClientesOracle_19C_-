using Backend.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services
{
    public sealed class JwtTokenService(IConfiguration config)
    {
        public LoginResponse Create(int idUsuario, string usuario)
        {
            string secret = config["Jwt:Secret"] 
                ?? throw new InvalidOperationException("FALTA la clave secreta Jwt:Secret!");

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            int minutes = int.TryParse(config["Jwt:ExpirationMinutes"], out var m) ? m : 30;
            DateTime expires = DateTime.UtcNow.AddMinutes(minutes);
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario)
            };
            
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: credentials);
            
            return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), usuario, expires);
        }
    }
}
