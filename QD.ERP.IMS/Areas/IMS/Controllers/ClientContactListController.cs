using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Web.Service;
using QD.ERP.Web.Services.Logging;
using SkiaSharp;

namespace QD.ERP.Web.Areas.IMS.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ClientContactListController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<ClientContactListController> _logger;
        private readonly IUserActionLogger _userActionLogger;

        public ClientContactListController(ILogger<ClientContactListController> logger, TenantDbContextHelper tenantDbContextHelper, IUserActionLogger userActionLogger)
        {
            _userActionLogger = userActionLogger;
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }
        [HttpGet]
        public IActionResult GetContactBySlNo(int clientContactSlNo)
        {
            try { 
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var data = dbContext.Tbl3010102clientContactLists.FirstOrDefault(x => x.ClientContactSlNo == clientContactSlNo);
                if (data != null)
                {
                    return Ok(data);
                }
                return NotFound(new { message = "Contact not found." });
            }
            return Unauthorized(new { message = "Invalid tenant." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting contact by SL No.");
                return StatusCode(500, new { message = "Internal server error." });
            }
        }

        [HttpPost]
        public IActionResult SaveOrUpdateClientContact([FromBody] Tbl3010102clientContactList item)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (item.ClientContactSlNo > 0)
                    {
                        // Update existing
                        var existing = dbContext.Tbl3010102clientContactLists
                            .FirstOrDefault(c => c.ClientContactSlNo == item.ClientContactSlNo);

                        if (existing != null)
                        {
                            // Update fields
                            existing.ClientCode = item.ClientCode;
                           // existing.ClientName = item.ClientName;
                            existing.ContactPerson = item.ContactPerson;
                            existing.ContactPersonTitle = item.ContactPersonTitle;
                            existing.ContactDivision = item.ContactDivision;
                            existing.ContactLocation = item.ContactLocation;
                            existing.ContactMobile1 = item.ContactMobile1;
                            existing.ContactPhone1 = item.ContactPhone1;
                            existing.ContactMobile2 = item.ContactMobile2;
                            existing.ContactPhone2 = item.ContactPhone2;
                            existing.ContactFaxNo = item.ContactFaxNo;
                            existing.ContactEmail = item.ContactEmail;

                            dbContext.SaveChanges();
                            _userActionLogger.LogAsync(module: "IMS > Save Or Update Client Contact",
                              actionDetail: $"Saved Client Contact {item.ClientCode}",
                              documentNo: $"{item.ClientCode}"
                            );

                            return Ok(new
                            {
                                success = true,
                                message = "Client contact updated successfully",
                                lastClientContactSlNo = existing.ClientContactSlNo
                            });
                        }
                        else
                        {
                            return NotFound(new { success = false, message = "Contact not found." });
                        }
                    }
                    else
                    {
                        // Insert new
                        dbContext.Tbl3010102clientContactLists.Add(item);
                        dbContext.SaveChanges();
                        _userActionLogger.LogAsync(module: "IMS > Save Or Update Client Contact",
                              actionDetail: $"Saved Client Contact {item.ClientCode}",
                              documentNo: $"{item.ClientCode}"
                            );

                        return Ok(new
                        {
                            success = true,
                            message = "Client contact saved successfully",
                            lastClientContactSlNo = item.ClientContactSlNo
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetProject: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "Error saving data: " + ex.Message });
                }
            }

            return Unauthorized(new { success = false, message = "Invalid tenant." });
        }


        //public IActionResult SaveClientCategory([FromBody] Tbl3010102clientContactList item)
        //{
        //    if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //    {
        //        try
        //    {
        //        dbContext.Tbl3010102clientContactLists.Add(item);
        //        dbContext.SaveChanges();
        //            return Ok(new
        //            {
        //                success = true,
        //                message = "Client Category saved successfully",
        //                lastClientContactSlNo = item.ClientContactSlNo
        //            });
        //        }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Error saving data: " + ex.Message });
        //    }
        //    }

        //    return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}
        [HttpPost]
        public IActionResult DeleteClientCategory([FromBody] long ClientContactSlNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
            {
                var entity = dbContext.Tbl3010102clientContactLists
                    .FirstOrDefault(x => x.ClientContactSlNo == ClientContactSlNo);

                if (entity == null)
                {
                    return NotFound(new { success = false, message = "Record not found." });
                }

                dbContext.Tbl3010102clientContactLists.Remove(entity);
                dbContext.SaveChanges();
                    _userActionLogger.LogAsync(module: "IMS > Delete Client Category",
                              actionDetail: $"Deleted Client Category {ClientContactSlNo}",
                              documentNo: $"{ClientContactSlNo}"
                            );

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


    }
}
