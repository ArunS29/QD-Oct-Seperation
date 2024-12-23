using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ExpensesClaimController : Controller
    {
        private ERPMasterWtDataContext _context;

        public ExpensesClaimController(ERPMasterWtDataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            try
            {

                var data = _context.Qry20121ExpenseClaimForms.Select(e => new
                {

                    e.ClaimRefNo,
                    ClaimDate = e.ClaimDate.HasValue
                    ? e.ClaimDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    e.ClaimerName,
                    e.PaymentVoucherNo,
                    BillDate = e.BillDate.HasValue
                    ? e.BillDate.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    e.BillRefNo,
                    e.ExpenseDescription,
                    e.ClaimedAmount,
                    e.ApprovedAmount,
                    e.CostCenterCode,
                    e.AccountHead,
                    e.IsTaxIncluded,
                    e.Discount,
                    e.TaxAmount,
                    e.RoundOff,
                    e.SupplierName,
                    e.SupplierVatno,
                    e.EmployeeNo,
                    e.EmployeeName,
                    e.PropertyNo,
                    e.PropertyDescription,
                    e.CostAllocationUnit,
                    e.PurchaserName
                }).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {

                return BadRequest(new { message = "An error occurred while fetching data.", error = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetExpensesClaims(DateTime? startDate, DateTime? endDate)
        {
            var vouchers = _context.Qry20121ExpenseClaimForms.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {

                vouchers = vouchers.Where(v => v.ClaimDate >= startDate && v.ClaimDate <= endDate);
            }

            return Ok(vouchers.ToList());
        }

    }
}