using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace QD.ERP.Shared.Service
{
    public class TenantRouteConstraint : IRouteConstraint
    {
        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out var value) && value is string tenant)
            {
                // Add your custom validation logic here
                return !string.IsNullOrWhiteSpace(tenant) && tenant.Length > 2;
            }
            return false;
        }
    }
}
