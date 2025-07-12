using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QD.ERP.Web.Service
{
    public class TokenRenewalMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        // Time before expiry to renew (e.g., 10 minutes)
        private const int RenewalWindowMinutes = 10;
        // New token lifetime (e.g., 60 minutes)
        private const int TokenLifetimeMinutes = 30;

        public TokenRenewalMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value.ToLower();

            // Exclude static and auth paths
            if (path.EndsWith(".js") || path.EndsWith(".css") || path.EndsWith(".png") ||
                path.EndsWith(".jpg") || path.EndsWith(".ico") || path.StartsWith("/static") ||
                path.Contains("/signin") || path.Contains("/licenseactivation"))
            {
                await _next(context);
                return;
            }

            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                try
                {
                    var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]);
                    var validationParams = new TokenValidationParameters
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

                    // Validate and read token
                    var principal = tokenHandler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                    var jwtToken = (JwtSecurityToken)validatedToken;

                    // Check expiry window
                    var exp = jwtToken.ValidTo;
                    var now = DateTime.UtcNow;
                    var minutesLeft = (exp - now).TotalMinutes;

                    if (minutesLeft < RenewalWindowMinutes && minutesLeft > 0)
                    {
                        // Generate new token with same claims
                        var claims = principal.Claims.ToList();
                        var newToken = GenerateJwtToken(claims, now, _configuration);

                        // Set cookie (overwrite old token)
                        context.Response.Cookies.Append("AuthToken", newToken, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            Expires = now.AddMinutes(TokenLifetimeMinutes),
                            SameSite = SameSiteMode.Lax,
                            Path = "/"
                        });
                    }
                }
                catch (Exception)
                {
                    // Ignore invalid/expired tokens, let other middleware handle auth
                }
            }

            await _next(context);
        }

        private string GenerateJwtToken(System.Collections.Generic.IEnumerable<Claim> claims, DateTime now, IConfiguration configuration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: configuration["JwtSettings:Issuer"],
                audience: configuration["JwtSettings:Audience"],
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(TokenLifetimeMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
