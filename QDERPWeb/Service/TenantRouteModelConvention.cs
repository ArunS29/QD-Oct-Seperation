using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace QD.ERP.Web.Service
{
    // This convention prepends "{tenantName}/" to all Razor Page routes
    public class TenantRouteModelConvention : IPageRouteModelConvention
    {
        public void Apply(PageRouteModel model)
        {
            foreach (var selector in model.Selectors)
            {
                var attributeRouteModel = selector.AttributeRouteModel;
                if (attributeRouteModel != null && !attributeRouteModel.Template.StartsWith("{tenantName}/"))
                {
                    attributeRouteModel.Template = "{tenantName}/" + attributeRouteModel.Template;
                }
            }
        }
    }
}
