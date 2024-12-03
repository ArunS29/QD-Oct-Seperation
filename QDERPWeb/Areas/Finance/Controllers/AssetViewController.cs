using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AssetViewController : Controller
    {
        private ERPMasterWtDataContext _context;

        public AssetViewController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult> GetAssetView()
        {
            try
            {

                ERPMasterWtDataContextProcedures _procedures = new ERPMasterWtDataContextProcedures(_context);
                var ledgerData = await _procedures.sp20157AssetRegisterViewAsync();

                return Json(ledgerData);

            }
            catch (Exception ex)
            {

                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
    }
}
