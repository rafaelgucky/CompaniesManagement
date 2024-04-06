using API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateToken(IEnumerable<Claim> claims, IConfiguration configuration)
        {
            // Super classe = SecurityTokenDescriptor

            /*
                Claims -> informações do usuário

                SecurityTokenDescriptor

                JwtSecurityTokenHandler
                JwtSecurityToken
             */
            string key = configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("Key was null");

            byte[] encodeKey = Encoding.ASCII.GetBytes(key);

            //SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(encodeKey), SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(configuration.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")),
                Audience = configuration["JWT:ValidAudience"],
                Issuer = configuration["JWT:ValidIssuer"],
                SigningCredentials =
                    new SigningCredentials(new SymmetricSecurityKey(encodeKey), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

            return handler.CreateJwtSecurityToken(descriptor);

            /*return handler.CreateJwtSecurityToken(configuration["JWT:ValidIssuer"],
                                           configuration["JWT:ValidAudience"],
                                           new ClaimsIdentity(claims),
                                           null,
                                           DateTime.UtcNow.AddMinutes(configuration.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")),
                                           null,
                                           new SigningCredentials(new SymmetricSecurityKey(encodeKey), SecurityAlgorithms.HmacSha256Signature)
                                           );
            */
        }

        public string GenerateRefreshToken()
        {
            // Super classe = RandomNumberGenerator

            var randomByte = new byte[128];

            RandomNumberGenerator.Create().GetBytes(randomByte);

            return Convert.ToBase64String(randomByte);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token, IConfiguration configuration)
        {

            // Super classe = TokenValidationParameters

            string secretKey = configuration["JWT:SecretKey"] ?? throw new ArgumentNullException("Key was null");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)) // Algorith ?
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
           
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return claims;
        }
    }
}
