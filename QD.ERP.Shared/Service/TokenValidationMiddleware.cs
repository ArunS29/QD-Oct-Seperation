using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Shared.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace QD.ERP.Shared.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _excludedAreas;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public TokenValidationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            IEnumerable<string> excludedAreas,
            IServiceScopeFactory scopeFactory,
            IMemoryCache cache)
        {
            _next = next;
            _configuration = configuration;
            _excludedAreas = excludedAreas;
            _scopeFactory = scopeFactory;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isApiRequest = context.Request.Path.StartsWithSegments("/api");

            var area = context.Request.RouteValues["area"]?.ToString();
            if (!string.IsNullOrEmpty(area) && _excludedAreas.Contains(area, StringComparer.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            if (isApiRequest && context.Request.Path.Value.ToLower().Contains("signin"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);

                try
                {
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration["JwtSettings:Issuer"],
                        ValidAudience = _configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var claims = jwtToken.Claims.ToList();

                    // Enforce single active session per client type: verify token JTI matches active session in cache
                    var username = claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
                    var tenantName = claims.FirstOrDefault(c => c.Type == "TenantName")?.Value;
                    var clientTypeFromToken = claims.FirstOrDefault(c => c.Type == "ClientType")?.Value;
                    var clientType = ResolveClientType(context, clientTypeFromToken);
                    var jti = jwtToken.Id;

                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(tenantName) && !string.IsNullOrWhiteSpace(jti))
                    {
                        var cacheKey = $"active_session:{tenantName.ToLower()}:{username.ToLower()}:{clientType}";
                        if (!_cache.TryGetValue(cacheKey, out SessionInfo activeSession) || !string.Equals(activeSession?.SessionId, jti, StringComparison.Ordinal))
                        {
                            HandleUnauthorized(context, isApiRequest, activeSession);
                            return;
                        }
                    }

                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Jwt"));

                    // 🔒 LICENSE CHECK
                    var companyName = claims.FirstOrDefault(c => c.Type == "CompanyName")?.Value;
                    if (!string.IsNullOrEmpty(companyName))
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var licenseService = scope.ServiceProvider.GetRequiredService<LicenseService>();

                        var isValid = await licenseService.IsLicenseValidAsync(companyName);
                        if (!isValid)
                        {
                            context.Response.Redirect("/pulse1/Security/LicenseActivation");
                            return;
                        }
                    }
                }
                catch (Exception)
                {
                    HandleUnauthorized(context, isApiRequest, null);
                    return;
                }
            }
            else
            {
                HandleUnauthorized(context, isApiRequest, null);
                return;
            }

            await _next(context);
        }

        private static string ResolveClientType(HttpContext context, string? clientTypeClaim)
        {
            if (!string.IsNullOrWhiteSpace(clientTypeClaim))
            {
                var norm = clientTypeClaim.Trim().ToLowerInvariant();
                return norm is "mobile" or "web" ? norm : "web";
            }

            var fromHeader = context.Request.Headers["X-Client-Type"].ToString();
            if (!string.IsNullOrWhiteSpace(fromHeader))
            {
                var norm = fromHeader.Trim().ToLowerInvariant();
                return norm is "mobile" or "web" ? norm : "web";
            }

            var ua = context.Request.Headers["User-Agent"].ToString().ToLowerInvariant();
            if (!string.IsNullOrEmpty(ua) && (ua.Contains("android") || ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("mobile")))
            {
                return "mobile";
            }

            return "web";
        }

        private void HandleUnauthorized(HttpContext context, bool isApiRequest, SessionInfo activeSession)
        {
            if (isApiRequest)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var deviceText = activeSession == null ? "Unauthorized" : $"Unauthorized. Active session on device: {activeSession.DeviceName ?? "Unknown"}, MAC: {activeSession.MacAddress ?? "Unknown"}";
                context.Response.WriteAsync($"{{\"message\":\"{deviceText}\"}}").Wait();
            }
            else
            {
                // For regular page requests, redirect to login but include an informational cookie
                // so the Login page can show an alert with the other device details.
                if (activeSession != null)
                {
                    var deviceText = $"You have been logged out because the same account was used on another device. Device: {activeSession.DeviceName ?? "Unknown"}, MAC: {activeSession.MacAddress ?? "Unknown"}.";
                    context.Response.Cookies.Append("ForcedLogoutMessage", deviceText, new CookieOptions
                    {
                        HttpOnly = false, // must be readable by JS on the Login page
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(2), // short-lived
                        Path = "/"
                    });
                }

                var tenantName = context.GetRouteValue("tenantName")?.ToString();
                var redirectUrl = string.IsNullOrEmpty(tenantName)
                    ? "/Aicon/security/login"
                    : $"/{tenantName}/Security/Login";

                context.Response.Redirect(redirectUrl);
            }
        }
    }
}
