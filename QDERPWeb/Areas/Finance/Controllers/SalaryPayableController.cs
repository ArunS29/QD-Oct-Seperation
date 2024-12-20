using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QDWEB.Areas.Finance.Controllers
{
  
    [Route("api/[controller]/[action]")]
   
    [ApiController]
    public class SalaryPayableController : Controller
    {
        private ERPMasterWtDataContext _context;

        public SalaryPayableController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetSalarybydate()
        {
            // Replace `YourEntity` with the actual entity name representing your data
            var vouchers = _context.Qry20196SalaryPayableByDateReports.Select(v => new
            {
                v.EmployeeNo,
                v.EmployeeName,
                v.NationalId,
                v.ReferenceNo,
                v.MonthOf,
                v.PayableAmount,
                //PayableAmount = v.PayableAmount < 0
                //? $"{Math.Abs(v.PayableAmount):N2}Cr"
                //: $"{v.PayableAmount:N2}",
                //v.Paid,
                v.Balance,
                //Balance = v.Balance.HasValue ? (v.Balance < 0 ? $"{Math.Abs(v.Balance.Value):N2}Cr"
                //: $"{v.Balance.Value:N2}")
                //: "0.00",

            }).ToList();

            return Json(vouchers);
        }
    }
}

