using Azure.Core;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
//using QD.ERP.Web.Models.DALCommon;
using QD.ERP.Web.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Communication.Email;
using QD.ERP.Web.Areas.Utility;
using System.Net.Mail;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;


namespace QD.ERP.Web.Areas.Security.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly DbContextFactory _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly LicenseService _licenseService;

        public LoginController(IMemoryCache cache, DbContextFactory dbContextFactory, IConfiguration configuration, LicenseService licenseService)
        {
            _cache = cache;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _licenseService = licenseService;
        }

        private bool TryGetTenantAndDbContext(string tenantName, out Tenant tenant, out ERPMasterWtDataContext dbContext)
        {
            tenant = null;
            dbContext = null;

            if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
                tenantCache.TryGetValue(tenantName.ToLower(), out tenant))
            {
                try
                {
                    dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return false;
        }

        private async Task<bool> IsLicenseValidAsync(Tenant tenant)
        {
            if (tenant == null || string.IsNullOrWhiteSpace(tenant.TenantName))
                return false;

            return await _licenseService.IsLicenseValidAsync(tenant.TenantName);
        }

        // Determine client type (web/mobile) without requiring model changes
        private string ResolveClientType()
        {
            var fromHeader = Request.Headers["X-Client-Type"].ToString();
            if (!string.IsNullOrWhiteSpace(fromHeader))
            {
                var norm = fromHeader.Trim().ToLowerInvariant();
                return norm is "mobile" or "web" ? norm : "web";
            }

            var ua = Request.Headers["User-Agent"].ToString().ToLowerInvariant();
            if (!string.IsNullOrEmpty(ua) && (ua.Contains("android") || ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("mobile")))
            {
                return "mobile";
            }

            return "web";
        }

        private string GetSessionCacheKey(string tenantName, string username)
        {
            return $"active_session:{tenantName?.ToLower()}:{username?.ToLower()}";
        }

        // Client-type aware cache key
        private string GetSessionCacheKey(string tenantName, string username, string clientType)
        {
            var type = string.IsNullOrWhiteSpace(clientType) ? "web" : clientType.ToLowerInvariant();
            return $"active_session:{tenantName?.ToLower()}:{username?.ToLower()}:{type}";
        }

        private void SetActiveSession(string tenantName, string username, string sessionId, TimeSpan ttl, SessionInfo sessionInfo)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username);
            _cache.Set(cacheKey, sessionInfo, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }

        // Client-type aware setter
        private void SetActiveSession(string tenantName, string username, string clientType, string sessionId, TimeSpan ttl, SessionInfo sessionInfo)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username, clientType);
            _cache.Set(cacheKey, sessionInfo, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            });
        }

        private SessionInfo GetActiveSessionInfo(string tenantName, string username)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username);
            return _cache.TryGetValue(cacheKey, out SessionInfo existing) ? existing : null;
        }

        // Client-type aware getter
        private SessionInfo GetActiveSessionInfo(string tenantName, string username, string clientType)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username, clientType);
            return _cache.TryGetValue(cacheKey, out SessionInfo existing) ? existing : null;
        }

        private string GetActiveSession(string tenantName, string username)
        {
            var info = GetActiveSessionInfo(tenantName, username);
            return info?.SessionId;
        }

        // Client-type aware session id getter
        private string GetActiveSession(string tenantName, string username, string clientType)
        {
            var info = GetActiveSessionInfo(tenantName, username, clientType);
            return info?.SessionId;
        }

        private void ClearActiveSession(string tenantName, string username)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username);
            _cache.Remove(cacheKey);
        }

        // Client-type aware clearer
        private void ClearActiveSession(string tenantName, string username, string clientType)
        {
            var cacheKey = GetSessionCacheKey(tenantName, username, clientType);
            _cache.Remove(cacheKey);
        }

        [HttpPost]
        public async Task<IActionResult> SignInAsync([FromBody] SignInRequest request)
        {
        

            if (request.ResetPassword)
            {
                if (TryGetTenantAndDbContext(request.TenantName, out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    if (!await IsLicenseValidAsync(tenant))
                    {
                        return Unauthorized(new
                        {
                            message = "License not valid."
                     
                        });
                    }


                    if (!string.IsNullOrEmpty(request.otp))
                    {
                        var passwordReset = dbContext.Passwordresets
                            .Where(pr => pr.Username == request.Username && pr.Otp == request.otp && pr.Status == true)
                            .ToList();

                        if (passwordReset.Any())
                        {
                            var user = dbContext.TblUserMasters
                                .FirstOrDefault(u => u.UserName == request.Username);

                            if (user != null)
                            {
                                user.Password = request.Password;
                                dbContext.SaveChanges();
                            }
                        }
                        else
                        {
                            return BadRequest(new { message = "Invalid OTP.", success = false });
                        }
                    }
                    else
                    {
                        string emailAddress = request.Username;

                        bool emailExists = dbContext.TblUserMasters.Any(user => user.UserName == emailAddress);

                        if (emailExists)
                        {
                            var random = new Random();
                            var otp = random.Next(100000, 999999).ToString();

                            String username = request.Username;

                            var passwordReset = new Passwordreset
                            {
                                Username = username,
                                Otp = otp,
                                ExpiryDateTime = DateTime.UtcNow.AddMinutes(5),
                                Status = true
                            };

                            dbContext.Set<Passwordreset>().Add(passwordReset);
                            dbContext.SaveChanges();

                            Task.Run(async () =>
                            {
                                await Task.Delay(TimeSpan.FromMinutes(5));
                                using (var scope = _dbContextFactory.CreateDbContext(tenant.ConnectionString))
                                {
                                    var resetEntry = scope.Passwordresets.FirstOrDefault(pr => pr.Username == username && pr.Otp == otp);
                                    if (resetEntry != null)
                                    {
                                        resetEntry.Status = false;
                                        scope.SaveChanges();
                                    }
                                }
                            });



                            EmailHelper.SendEmailAsync(emailAddress, "OTP Verification", $"<h1>Your OTP is: {otp}</h1>").Wait();
                        }
                        else
                        {
                            return BadRequest(new { message = "Email does not exist.", success = false });
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.TenantName) ||
                    string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { message = "All fields are required.", success = false });
                }

                if (TryGetTenantAndDbContext(request.TenantName, out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    if (!await IsLicenseValidAsync(tenant))
                    {
                        return Unauthorized(new
                        {
                            message = "License not valid."

                        });
                    }


                    using (dbContext)
                    {
                        var user = dbContext.TblUserMasters
                            .FirstOrDefault(u => u.UserName == request.Username && u.Password == request.Password);

                        if (user == null)
                        {
                            return Unauthorized(new { message = "Invalid credentials.", success = false });
                        }

                        var clientType = ResolveClientType();

                        // Allow device 2 to login and force device 1 out by overriding active session (per client type)
                        var previousInfo = GetActiveSessionInfo(request.TenantName, request.Username, clientType);

                        var sessionId = Guid.NewGuid().ToString();
                        var tokenTtl = TimeSpan.FromMinutes(20);
                        var sessionInfo = new SessionInfo
                        {
                            SessionId = sessionId,
                            DeviceName = string.IsNullOrWhiteSpace(request.DeviceName) ? Request.Headers["X-Device-Name"].ToString() : request.DeviceName,
                            MacAddress = string.IsNullOrWhiteSpace(request.MacAddress) ? Request.Headers["X-Mac-Address"].ToString() : request.MacAddress,
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                        };

                        // Override any existing session for this client type only
                        SetActiveSession(request.TenantName, request.Username, clientType, sessionId, tokenTtl, sessionInfo);

                        var permissions = dbContext.TblUserAccessWebs
                            .Where(p => p.UserId == user.UserId)
                            .Select(p => new Permission
                            {

                                UserId = p.UserId,
                                ItemForm = p.ItemForm,
                                ItemName = p.ItemName,
                                ItemEnabled = p.ItemEnabled,
                                ItemVisible = p.ItemVisible
                            }).ToList();

                        var token = GenerateJwtToken(user, request.TenantName, sessionId, clientType);

                        SetHttpOnlyCookie("AuthToken", token, 20);

                        HttpContext.Session.SetString("TenantName", request.TenantName);
                        HttpContext.Session.SetString("UserName", request.Username);
                        HttpContext.Session.SetString("DefaultcompanyID", tenant.DefaultcompanyID);
                        HttpContext.Session.SetString("currencytype", tenant.currencytype);
                        HttpContext.Session.SetString("currencyID", tenant.currencyID);
                        HttpContext.Session.SetString("baseCurrencyname", tenant.baseCurrencyname);
                        HttpContext.Session.SetString("UserId", user.UserId.ToString());

                        return Ok(new
                        {
                            message = previousInfo == null
                                ? "Login successful"
                                : $"Login successful. Previous device forced to logout (Device: {previousInfo?.DeviceName ?? "Unknown"}, MAC: {previousInfo?.MacAddress ?? "Unknown"}).",
                            success = true,
                            token,
                            permissions

                        });
                    }
                }
                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            return Ok(new
            {
                message = "Login successful",
                success = true,

            });
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
                var clientTypeFromToken = principal.Claims.FirstOrDefault(c => c.Type == "ClientType")?.Value;
                var clientType = string.IsNullOrWhiteSpace(clientTypeFromToken) ? ResolveClientType() : clientTypeFromToken.ToLowerInvariant();
                var jti = jwtToken.Id;

                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantName) || string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized(new { message = "Invalid token data.", success = false });
                }

                // Enforce single session per client type: ensure this token's jti matches active session
                var activeSession = GetActiveSession(tenantName, username, clientType);
                if (string.IsNullOrEmpty(activeSession) || !string.Equals(activeSession, jti, StringComparison.Ordinal))
                {
                    return Unauthorized(new { message = "Session expired or logged in from another device.", success = false });
                }

                if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
               tenantCache.TryGetValue(tenantName.ToLower(), out Tenant tenant))
                {
                    using (var dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString))
                    {

                        var permissions = dbContext.TblUserAccessWebs
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
                
                // Rotate session id and token
                var newSessionId = Guid.NewGuid().ToString();
                var newSessionInfo = new SessionInfo
                {
                    SessionId = newSessionId,
                    DeviceName = Request.Headers["X-Device-Name"].ToString(),
                    MacAddress = Request.Headers["X-Mac-Address"].ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                SetActiveSession(tenantName, username, clientType, newSessionId, TimeSpan.FromMinutes(20), newSessionInfo);
                var newToken = GenerateJwtToken(user, tenantName, newSessionId, clientType);
                SetHttpOnlyCookie("Permissions", JsonSerializer.Serialize(permissions), 20);
                SetHttpOnlyCookie("AuthToken", newToken, 20);

                        var sessionCookie = Request.Cookies[".AspNetCore.Session"];
                      

                        return Ok(new { message = "Session extended successfully.", success = true, token = newToken, sessionCookie, permissions= permissions });
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
            try
            {
                var existingToken = HttpContext.Request.Cookies["AuthToken"];
                if (!string.IsNullOrEmpty(existingToken))
                {
                    var principal = ValidateToken(existingToken, out var jwtToken);
                    if (principal != null && jwtToken != null)
                    {
                        var username = principal.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                        var tenantName = principal.Claims.FirstOrDefault(c => c.Type == "TenantName")?.Value;
                        var clientTypeFromToken = principal.Claims.FirstOrDefault(c => c.Type == "ClientType")?.Value;
                        var clientType = string.IsNullOrWhiteSpace(clientTypeFromToken) ? ResolveClientType() : clientTypeFromToken.ToLowerInvariant();
                        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(tenantName))
                        {
                            // Clear only this client type session
                            ClearActiveSession(tenantName, username, clientType);
                        }
                    }
                }
            }
            catch { /* ignore */ }

            HttpContext.Response.Cookies.Delete("AuthToken");
            HttpContext.Session.Clear();
            return Ok(new { message = "Sign-out successful.", success = true });
        }

        [HttpGet]
        public IActionResult ConnectionString(string tenantName)
        {
            // Ensure the tenant name is not null or empty.
            if (string.IsNullOrWhiteSpace(tenantName))
            {
                return BadRequest(new { message = "Tenant name is required.", success = false });
            }

            // Attempt to fetch the tenant object from the cache.
            if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
                tenantCache.TryGetValue(tenantName.ToLower(), out Tenant tenant))
            {
                return Ok(new { connectionString = tenant.ConnectionString });
            }

            // Return an error if the tenant is not found.
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        private string GenerateJwtToken(TblUserMaster user, string tenantName, string sessionId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("UserName", user.UserName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("TenantName", tenantName),
                new Claim(JwtRegisteredClaimNames.Jti, sessionId)
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

        // Client-type aware token generator
        private string GenerateJwtToken(TblUserMaster user, string tenantName, string sessionId, string clientType)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("UserName", user.UserName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("TenantName", tenantName),
                new Claim(JwtRegisteredClaimNames.Jti, sessionId),
                new Claim("ClientType", string.IsNullOrWhiteSpace(clientType) ? "web" : clientType.ToLowerInvariant())
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
