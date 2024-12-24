using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using NuGet.Protocol;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    //[ApiController]
    public class JournalRegisterController : Controller
    {
        private ERPMasterWtDataContext _context;
        public JournalRegisterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetJournalview(byte RequesterID, DateTime StartDate, DateTime EndDate, bool IfShowAll)
        {
            try
            {
                ERPMasterWtDataContextProcedures procedure = new ERPMasterWtDataContextProcedures(_context);
                var result = await procedure.sp20201JournalRegisterViewAsync(RequesterID, StartDate, EndDate, IfShowAll);

                if (result != null && result.Any())
                {
                    return Json(result);
                }

                return Json(new { success = false, message = "No data found." });
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while fetching the data." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetUser()
        {
            try
            {
                // Fetch user data from the database
                var users = await _context.TblUserMasters
                    .Select(u => new
                    {
                        u.UserId,
                        u.UserName // Ensure this is a valid property
                    })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                // Log the error (implement a logger like Serilog or NLog)
                Console.WriteLine($"Error fetching users: {ex.Message}");
                return Json(new { error = "Unable to fetch user data at this time." });
            }
        }

        [HttpPost]
        public ActionResult UpdateData(QD.ERP.Web.DAL.Entities.sp20201JournalRegisterViewResult updatedItem)
        {
            // Perform the update logic here.
            // Example: Update the item in the database.

            return Json(updatedItem);
        }
    }
}