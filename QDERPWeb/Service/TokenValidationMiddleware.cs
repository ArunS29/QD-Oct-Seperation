using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Web.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QD.ERP.Web.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _excludedAreas;
        private readonly IServiceScopeFactory _scopeFactory;

        public TokenValidationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            IEnumerable<string> excludedAreas,
            IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _configuration = configuration;
            _excludedAreas = excludedAreas;
            _scopeFactory = scopeFactory;
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
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var claims = jwtToken.Claims.ToList();
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
                    HandleUnauthorized(context, isApiRequest);
                    return;
                }
            }
            else
            {
                HandleUnauthorized(context, isApiRequest);
                return;
            }

            await _next(context);
        }

        private void HandleUnauthorized(HttpContext context, bool isApiRequest)
        {
            if (isApiRequest)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync("{\"message\":\"Unauthorized\"}").Wait();
            }
            else
            {
                var tenantName = context.GetRouteValue("tenantName")?.ToString();
                var redirectUrl = string.IsNullOrEmpty(tenantName)
                    ? "/pulse/Security/Login"
                    : $"/{tenantName}/Security/Login";

                context.Response.Redirect(redirectUrl);
            }
        }
    }
}
