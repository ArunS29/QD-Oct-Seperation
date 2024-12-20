using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

               
                var result = ledgerData.Select(x => new
                {
                    x.AccountGroup,
                    x.AssetLedgerNo,
                    x.AccountHead,
                    x.AssetDescription,
                    x.Specifications,
                    x.PropertyNo,
                    x.DepreciationMethod,
                    x.ScrapValueOfProperty,
                    x.LifeSpanOfProperty,
                    x.DepreciationPercentage,
                    x.OpeningTotal,
                    x.TotalDebit,
                    x.TotalCredit,
                    //TotalCredit = x.TotalCredit.HasValue ? (x.TotalCredit < 0 ? $"{Math.Abs(x.TotalCredit.Value):N2}Cr" 
                    //: $"{x.TotalCredit.Value:N2}") 
                    //: "0.00",
                    x.ClosingBalance,
                    x.TotalDepreciatedAmount,
                    x.NetBookValue,
                    x.AssetCategory,
                    x.AssetLocation,
                    x.Brand,
                    x.PlateNo,
                    x.Model,
                    x.Year,
                    x.Ownership,
                     PurchaseDate = x.PurchaseDate.HasValue
                    ? x.PurchaseDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    x.ValueOfProperty,
                    x.PurchasedAs,
                    x.IsFinanced,
                    x.FinancedFrom,
                    x.NoOfInstallments,
                    x.InitialDownPayment,
                    x.InitialDocCharges,
                    x.MonthlyInstallment,
                    x.FinalInstallment,
                     InstallmentStartDate = x.InstallmentStartDate.HasValue
                    ? x.InstallmentStartDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    InstallmentEndDate = x.InstallmentEndDate.HasValue
                    ? x.InstallmentEndDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                }).ToList();


               
                return Json(result);
            }
            catch (Exception ex)

            {
               
                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
              
                var result = _context.Database.ExecuteSqlRaw("EXEC sp20121DeleteAssetRegister @Id = {0}", id);

                
                if (result == 0)
                    return NotFound(new { message = "Asset not found or could not be deleted." });

               
                return Ok(new { message = "Asset deleted successfully." });
            }
            catch (Exception ex)
            {
     
                return StatusCode(500, new { message = "An error occurred while deleting the asset.", error = ex.Message });
            }
        }

    }
}
