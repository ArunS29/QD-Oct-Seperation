using System.IdentityModel.Tokens.Jwt;

namespace QD.ERP.Shared.Service
{
    public class JwtTokenHelper
    {
        /// <summary>
        /// Parses a JWT token and returns claims as a dynamic object.
        /// </summary>
        /// <param name="jwtToken">The JWT token string.</param>
        /// <returns>A dynamic object containing the claims or null if the token is invalid.</returns>
        public static dynamic GetClaimsFromToken(string jwtToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                // Ensure the token is valid
                if (!handler.CanReadToken(jwtToken))
                    throw new ArgumentException("Invalid JWT token.");

                // Read the token
                var token = handler.ReadJwtToken(jwtToken);

                // Extract claims
                var claims = token.Claims.ToDictionary(claim => claim.Type, claim => claim.Value);

                // Return as a dynamic object
                return new
                {
                    Claims = claims,
                    Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims["exp"])),
                    IssuedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims["iat"])),
                    Token = jwtToken
                };
            }
            catch (Exception ex)
            {
                // Handle invalid token errors
                Console.WriteLine($"Error reading JWT token: {ex.Message}");
                return null;
            }
        }
    }
}
