using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Areas.IMS.Controllers;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientLeadsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientLeadsController> _logger;


        public ClientLeadsController(ILogger<ClientLeadsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var clients = await dbContext.Qry30101ClientLists
                        .Select(i => new
                        {
                            i.ClientCode,
                            i.ClientName,
                            i.ContactPerson,
                            i.ContactMobile1,
                            i.ContactPhone1,
                            i.ClientLedgerNo,
                            BusinessCard1 = i.BusinessCard1 != null ? Convert.ToBase64String(i.BusinessCard1) : null,


                          
                            i.ClientCategory,
                            i.ContactPhone2,
                            i.ClientNameAr,
                            i.ClientAddress,
                            i.ContactMobile2,                          
                            i.ContactEmail,
                            i.ContactFaxNo,
                            i.ContactRemarks,
                            i.IsDiscontinued,
                            i.ReasonDiscontinued,
                            i.CreatedBy,
                            i.CreatedOn,
                            i.ModifiedBy,
                            i.ModifiedOn,
                            i.DiscontinuedBy,
                            i.DiscontinuedOn,
                            i.DateVisitedFirst,
                            i.Category,
                            i.ReportedBy,
                            i.ReportedOn,
                            i.StatusRemarks,
                            i.FollowupOn,
                            i.Status,
                     
                            i.SalesPersonCode,
                            i.SalesPersonName,
                            i.UserCode,
                            i.VendorNo,
                        
                            i.ClientLedgerName,
                          
                            i.BusinessCard2,

                        })
                        .ToListAsync();

                    return Json(clients); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }



    }
}