using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.ERM.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientLeadMaster1Controller : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientLeadMaster1Controller> _logger;

        public ClientLeadMaster1Controller(ILogger<ClientLeadMaster1Controller> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientCategory(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientCategory = dbContext.Tbl30102ClientCategories.Select(i => new
                    {
                        i.CategoryCode,
                        i.ClientCategory,
                        i.ClientCategoryCode
                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientCategory, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public IActionResult GetLatestClientCode(string categoryCode)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var latestClientCode = dbContext.Tbl30101ClientMasters
                        .Where(c => c.ClientCode.StartsWith(categoryCode + "-"))
                        .OrderByDescending(c => c.ClientCode)
                        .Select(c => c.ClientCode)
                        .FirstOrDefault();

                    return Ok(latestClientCode); // returns e.g., "SW-4"
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLatestClientCode: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", ex });
            }
                return Unauthorized(new { message = "Invalid tenant." });
        }


        [HttpGet]
        public async Task<IActionResult> GetClientLedgerNo(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientLedgerNo = dbContext.Qry20110SundryDebtors.Select(i => new
                    {
                        i.AccountId,
                        i.AccountHead,
                    
                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientLedgerNo, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesPerson(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var ClientLedgerNo = dbContext.Tbl20101SalesPersonMasters.Select(i => new
                    {
                        i.SalesPersonCode,
                        i.SalesPersonName,

                    });

                    return Json(await DataSourceLoader.LoadAsync(ClientLedgerNo, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }





        [HttpPost]
        public IActionResult InsertOrUpdate([FromBody] Tbl30101ClientMaster clientMaster)
        {
            if (clientMaster == null)
            {
                return BadRequest("Invalid client data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var existingClient = dbContext.Tbl30101ClientMasters
                        .FirstOrDefault(c => c.ClientCode == clientMaster.ClientCode);

                    if (existingClient != null)
                    {
                        // Update existing record
                        existingClient.ClientName = clientMaster.ClientName;
                        existingClient.ClientCategory = clientMaster.ClientCategory;
                        existingClient.DateVisitedFirst = clientMaster.DateVisitedFirst;
                        existingClient.ClientAccountLedgerNo = clientMaster.ClientAccountLedgerNo;
                        existingClient.ClientAddress = clientMaster.ClientAddress;
                        existingClient.ContactPerson = clientMaster.ContactPerson;
                        existingClient.ContactPersonTitle = clientMaster.ContactPersonTitle;
                        existingClient.ContactMobile1 = clientMaster.ContactMobile1;
                        existingClient.ContactMobile2 = clientMaster.ContactMobile2;
                        existingClient.ContactPhone1 = clientMaster.ContactPhone1;
                        existingClient.ContactPhone2 = clientMaster.ContactPhone2;
                        existingClient.ContactFaxNo = clientMaster.ContactFaxNo;
                        existingClient.ContactEmail = clientMaster.ContactEmail;
                        existingClient.SalesPersonCode = clientMaster.SalesPersonCode;
                        existingClient.VendorNo = clientMaster.VendorNo;
                        existingClient.ContactRemarks = clientMaster.ContactRemarks;


                        // Handle image fields (update them as byte[] or Base64)
                        if (clientMaster.BusinessCard1 != null)
                            existingClient.BusinessCard1 = clientMaster.BusinessCard1;

                        if (clientMaster.BusinessCard2 != null)
                            existingClient.BusinessCard2 = clientMaster.BusinessCard2;



                        dbContext.Tbl30101ClientMasters.Update(existingClient);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Client updated successfully." });
                    }
                    else
                    {
                        // Insert new record
                        var newClient = new Tbl30101ClientMaster
                        {
                            ClientCode = clientMaster.ClientCode,
                            ClientName = clientMaster.ClientName,
                            ClientCategory = clientMaster.ClientCategory,
                            DateVisitedFirst = clientMaster.DateVisitedFirst,
                            ClientAccountLedgerNo = clientMaster.ClientAccountLedgerNo,
                            ClientAddress = clientMaster.ClientAddress,
                            ContactPerson = clientMaster.ContactPerson,
                            ContactPersonTitle = clientMaster.ContactPersonTitle,
                            ContactMobile1 = clientMaster.ContactMobile1,
                            ContactMobile2 = clientMaster.ContactMobile2,
                            ContactPhone1 = clientMaster.ContactPhone1,
                            ContactPhone2 = clientMaster.ContactPhone2,
                            ContactFaxNo = clientMaster.ContactFaxNo,
                            ContactEmail = clientMaster.ContactEmail,
                            SalesPersonCode = clientMaster.SalesPersonCode,
                            VendorNo = clientMaster.VendorNo,
                            ContactRemarks = clientMaster.ContactRemarks,
                            // Handle image fields (insert them as byte[] or Base64)
                            BusinessCard1 = clientMaster.BusinessCard1,
                            BusinessCard2 = clientMaster.BusinessCard2

                        };

                        dbContext.Tbl30101ClientMasters.Add(newClient);
                        dbContext.SaveChanges();

                        return Ok(new { success = true, message = "Client saved successfully." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error saving ClientMaster: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpPost]
        public IActionResult DeleteClientLeadMaster([FromBody] string ClientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var entity = dbContext.Tbl30101ClientMasters
                        .FirstOrDefault(x => x.ClientCode == ClientCode);

                    if (entity == null)
                    {
                        return NotFound(new { success = false, message = "Record not found." });
                    }

                    dbContext.Tbl30101ClientMasters.Remove(entity);
                    dbContext.SaveChanges();

                    return Ok(new { success = true, message = "Deleted successfully." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { success = false, message = $"Delete failed: {ex.Message}" });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetClientByCode(string clientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                if (string.IsNullOrEmpty(clientCode))
                return BadRequest("Client Code is required.");

            try
            {
             
                var client = await dbContext.Tbl30101ClientMasters
                    .Where(c => c.ClientCode == clientCode)
                    .FirstOrDefaultAsync();

                if (client == null)
                    return NotFound("Client not found.");

                return Ok(client);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }





        [HttpGet]
        public async Task<IActionResult> GetClientStatus(string clientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Qry30102ClientStatuses
                        .Where(i => i.ClientCode == clientCode)
                        .ToListAsync();

                    return Json(result);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized();
        }


        [HttpGet]
       public async Task<IActionResult> GetContactList(string clientCode)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var result = await dbContext.Tbl3010102clientContactLists
                      .Where(i => i.ClientCode == clientCode)
                      .ToListAsync();

                    return Json(result);
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpDelete]
        public IActionResult DeleteContact(long clientContactSlNo)
        {
                try
                {
                    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    {
                        var contact = dbContext.Tbl3010102clientContactLists
                            .FirstOrDefault(c => c.ClientContactSlNo == clientContactSlNo);

                        if (contact != null)
                        {
                            dbContext.Tbl3010102clientContactLists.Remove(contact);
                            dbContext.SaveChanges();
                            return Ok(new { success = true, message = "Contact deleted successfully." });
                        }

                        return NotFound(new { success = false, message = "Contact not found." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching the data.", error = ex.Message });
                }
            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }

        [HttpDelete]
        public IActionResult DeleteStatus(long clientStatusNo)
        {
                try
                {
                    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    {
                        var status = dbContext.Tbl30104ClientStatuses
                            .FirstOrDefault(s => s.ClientStatusNo == clientStatusNo);

                        if (status != null)
                        {
                            dbContext.Tbl30104ClientStatuses.Remove(status);
                            dbContext.SaveChanges();
                            return Ok(new { success = true, message = "Client status deleted successfully." });
                        }

                        return NotFound(new { success = false, message = "Status not found." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while fetching the data.", ex});
            }
            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }
        [HttpGet]
        public IActionResult GetClientDetails(string clientCode)
        {
            if (string.IsNullOrEmpty(clientCode))
                return BadRequest("Client code is required.");

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var client = dbContext.Tbl30101ClientMasters
                    .FirstOrDefault(c => c.ClientCode == clientCode);

                if (client == null)
                    return NotFound("Client not found.");

                return Ok(client); // This returns all client fields
            }

            return Unauthorized("Invalid tenant.");
        }

    }
}
