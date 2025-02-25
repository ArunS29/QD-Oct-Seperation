using Azure.Core;
using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.Service;
using QD.ERP.Web.Models.DALCommon;
using QD.ERP.Web.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace QD.ERP.Web.Areas.Security.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly DbContextFactory _dbContextFactory;
        private readonly IConfiguration _configuration;

        public LoginController(IMemoryCache cache, DbContextFactory dbContextFactory, IConfiguration configuration)
        {
            _cache = cache;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult SignIn([FromBody] SignInRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenantName) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "All fields are required.", success = false });
            }

            if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
                tenantCache.TryGetValue(request.TenantName.ToLower(), out Tenant tenant))
            {
                try
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString))
                    {
                        var user = dbContext.TblUserMasters
                            .FirstOrDefault(u => u.UserName == request.Username && u.Password == request.Password);

                        if (user == null)
                        {
                            return Unauthorized(new { message = "Invalid credentials.", success = false });
                        }

                        var permissions = dbContext.TblUserAccesses
                            .Where(p => p.UserId == user.UserId)
                            .Select(p => new Permission
                            {
                                UserId = p.UserId,
                                ItemForm = p.ItemForm,
                                ItemName = p.ItemName,
                                ItemEnabled = p.ItemEnabled,
                                ItemVisible = p.ItemVisible
                            })
                            .ToList();

                        var token = GenerateJwtToken(user, request.TenantName);

                        SetHttpOnlyCookie("AuthToken", token, 20);
                        SetHttpOnlyCookie("Permissions", JsonSerializer.Serialize(permissions), 20);

                        return Ok(new
                        {
                            message = "Login successful",
                            success = true,
                            token,
                            permissions
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred during login.", success = false });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult ExtendSession()
        {
            var existingToken = HttpContext.Request.Cookies["AuthToken"];
            if (string.IsNullOrWhiteSpace(existingToken))
            {
                return Unauthorized(new { message = "No token found. Please log in again.", success = false });
            }

            try
            {
                var principal = ValidateToken(existingToken, out var jwtToken);
                if (principal == null || jwtToken == null)
                {
                    return Unauthorized(new { message = "Invalid token. Please log in again.", success = false });
                }
                var username = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                var userId = principal.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var tenantName = principal.Claims.FirstOrDefault(c => c.Type == "TenantName")?.Value;

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantName))
                {
                    return Unauthorized(new { message = "Invalid token data.", success = false });
                }
                if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
               tenantCache.TryGetValue(tenantName.ToLower(), out Tenant tenant))
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString))
                    {

                        var permissions = dbContext.TblUserAccesses
                    .Where(p => p.UserId == byte.Parse(userId))
                    .Select(p => new Permission
                    {
                        UserId = p.UserId,
                        ItemForm = p.ItemForm,
                        ItemName = p.ItemName,
                        ItemEnabled = p.ItemEnabled,
                        ItemVisible = p.ItemVisible
                    })
                    .ToList();
                        

                    
                    var user = new TblUserMaster { UserId = byte.Parse(userId), UserName = username };
                var newToken = GenerateJwtToken(user, tenantName);
                SetHttpOnlyCookie("Permissions", JsonSerializer.Serialize(permissions), 20);
                SetHttpOnlyCookie("AuthToken", newToken, 20);

                        return Ok(new { message = "Session extended successfully.", success = true, token = newToken, permissions= permissions });
                    }
                }
                else
                {
                    return StatusCode(500, new { message = "An error occurred while extending the session.", success = false });
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized(new { message = "Token expired. Please log in again.", success = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while extending the session.", success = false });
            }
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            HttpContext.Response.Cookies.Delete("AuthToken");
            return Ok(new { message = "Sign-out successful.", success = true });
        }

        [HttpGet]
        public IActionResult ConnectionString(string tenantName)
        {
            if (_cache.TryGetValue(tenantName, out string connectionString))
            {
                return Ok(new { connectionString });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        private string GenerateJwtToken(TblUserMaster user, string tenantName)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("UserName", user.UserName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("TenantName", tenantName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal ValidateToken(string token, out JwtSecurityToken jwtToken)
        {
            jwtToken = null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                jwtToken = validatedToken as JwtSecurityToken;

                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private void SetHttpOnlyCookie(string key, string value, int hoursToExpire)
        {
            HttpContext.Response.Cookies.Append(key, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(hoursToExpire)
            });
        }
    }
}
