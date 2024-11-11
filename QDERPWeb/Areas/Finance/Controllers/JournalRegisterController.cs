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
            ERPMasterWtDataContextProcedures procedure = new ERPMasterWtDataContextProcedures(_context);
            var result = await procedure.sp20201JournalRegisterViewAsync(RequesterID, StartDate, EndDate, IfShowAll);
            return Json(result);
        }
        [HttpGet]
        public async Task<ActionResult> GetUser()
        {

            return Json(_context.TblUserMasters.ToList());
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
