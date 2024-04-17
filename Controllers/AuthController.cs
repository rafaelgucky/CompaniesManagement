using API.DTOs;
using API.Models;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login([FromBody] LoginModelDTO loginModelDTO)
        {
            /*
             * 
            Roles = autorizações
            Claims = dados do usuário que vão ser encriptados no token

             */
            var user = await _userManager.FindByNameAsync(loginModelDTO.Name!);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginModelDTO.Password!))
            {
                // Obtendo os roles (autorizações) e claims (informações) do usuário
                var userRoles = await _userManager.GetRolesAsync(user);
                
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
                userRoles.ToList().ForEach(ur =>
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, ur));
                });

                // Criando o token
                var token = _tokenService.GenerateToken(authClaims, _configuration);

                // Criando o refresh token
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Obtendo o tempo de expiração do refresh token
                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

                // Configurando o tempo de expiração do refresh token do usuário
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

                // Configurando o refresh token do usuário
                user.RefreshToken = refreshToken;

                // Atualizando o usuário
                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }
            return Unauthorized("You are not autorized");
        }

        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] RegisterModelDTO registerModelDTO)
        {
            var userVerification = await _userManager.FindByNameAsync(registerModelDTO.Name!);
            if (userVerification != null) return BadRequest("User already exists!");

            var user = new ApplicationUser
            {
                UserName = registerModelDTO.Name,
                Email = registerModelDTO.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, registerModelDTO.Password!);

            if (!result.Succeeded) return BadRequest("Not created");

            return Ok("User created");
        }
        [HttpPost]
        [Route("refresh-token")]
        public async Task<ActionResult> RefreshToken(TokenModelDTO tokenModelDTO)
        {
            /*
             
            Obter as claims pricipais através dos dos parâmetros recebidos
            Essas claim vão conter os valores necessários para obter um usuário e gerar um novo token e refresh token
            
             */

            // Verifacação dos dados de entrada
            if (tokenModelDTO == null) return BadRequest("Invalid operation");

            // Ter certeza de que os valores vindos não são nulos
            string? acessToken = tokenModelDTO.AccessToken ?? throw new ArgumentNullException("Acess token can't be null");
            string? refreshToken = tokenModelDTO.RefreshToken ?? throw new ArgumentNullException("Acess token can't be null");

            // Obtendo as claims do usuário com o accestoken expirado [se existir]
            var claims = _tokenService.GetPrincipalFromExpiredToken(acessToken!, _configuration);

            // Verificação dos dados decodificados pelo serviço dos tokens
            if (claims == null) return BadRequest("Invalid token");

            // Obtendo usuário com as claim descodificadas
            var user = await _userManager.FindByNameAsync(claims.Identity!.Name!);

            // Validação
            if (user == null || user!.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now) return BadRequest("Incorret refresh token");

            // Gerando um novo access token e refresh token
            var newAcessToken = _tokenService.GenerateToken(claims.Claims.ToList(), _configuration);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            // Atualizando as informações do usuário e perssitindo no banco
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newAcessToken),
                refreshToken = newRefreshToken
            });
        }
        [Authorize]
        [HttpPost]
        [Route("revoke/{userName}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> Revoke(string userName)
        {
            // Revogar um usuário com base no nome

            // Obter usuário do banco através do nome
            var user = await _userManager.FindByNameAsync(userName);

            // Verificação de null
            if (user == null) return BadRequest("Invalid user name");

            // Atualização em memória do refresh token
            user.RefreshToken = null;

            // Persistindo os dados
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        [HttpPost]
        [Route("CreateRole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateRole(string role)
        {
            if (await _roleManager.RoleExistsAsync(role)) return BadRequest("Role already exists!");

            IdentityResult result = await _roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded) return BadRequest("Ocurred an error during creation");

            return Ok("Created");
        }

        [HttpPost]
        [Route("connectusertorole")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> ConnectUserToRole(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null) return BadRequest("User not found");

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded) return BadRequest("Unable to connect");

            return Ok("Connected");
        }
    }
}
