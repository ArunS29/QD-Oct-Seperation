using QD.ERP.Web.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UtilityController : Controller
    {
        private ERPMasterWtDataContext _context;
        public UtilityController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<ActionResult> SaveLayout(string layout, string form)
        {

            ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
            var ledgerData = await _procedures.sp901_01UpdateLayoutAsync(layout, form, "101", true);
            return Json(ledgerData);
        }
        [HttpGet]
        public async Task<ActionResult> LoadLayout(string form)
        {

            var layout = _context.Tbl90111LayoutMasters.Where(p => p.UserId == 101 && p.FormId == form).Select(i => new
            {
                i.LayoutJson
            }).FirstOrDefault();
            if (layout != null && layout.LayoutJson != null)
            {
                return Json(layout.LayoutJson);
            }
            else
            {
                return Json(null); // or return some default value if appropriate
            }
        }
    }
}
