//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using QD.ERP.Web.DAL.Entities;

//namespace QD.ERP.Web.Areas.Finance.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AssetAllocationController : ControllerBase
//    {
//        private ERPMasterWtDataContext _context;

//        public AssetAllocationController(ERPMasterWtDataContext context)
//        {
//            _context = context;
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AssetAllocationController : Controller
	{
		private ERPMasterWtDataContext _context;

		public AssetAllocationController(ERPMasterWtDataContext context)
		{
			_context = context;
		}


        [HttpGet]
        public async Task<IActionResult> GetAssetCostAllocations()
        {
            try
            {
                var data = await _context.Tbl20117AssetCostAllocationMasters
                    .Select(a => new
                    {
                        a.AssetCostAllocationId,
                        a.AssetCostAllocDrCr,
                        a.AssetNo,
                        a.EffectiveDate,
                        a.AmountAllocated,
                        a.AssetCostAllocRemarks
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

    }
}
