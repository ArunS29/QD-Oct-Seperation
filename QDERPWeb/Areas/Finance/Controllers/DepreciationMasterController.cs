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
    public class DepreciationMasterController : Controller
    {
        private ERPMasterWtDataContext _context;

        public DepreciationMasterController(ERPMasterWtDataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult GetExpenseClaims()
        {
            try
            {

                var data = _context.Qry201205depreciationMasterViews.Select(e => new
                {

                    e.JournalVoucherNo,
                    e.DepreciationDocNo,
                    e.DeprStartDate,
                    e.DeprEndDate,
                    e.NoOfAssets,
                    e.AssetOpeningBalance,
                    e.AssetTotalDebitTrans,
                    Postedon = e.PostedOn.HasValue
                    ? e.PostedOn.Value.ToString("dd-MMM-yyyy")
                    : string.Empty,
                    e.AssetTotalCreditTrans,
                    e.AssetClosingBalance,
                    e.AccumulatedOpeningBalance,
                    e.TotalDepreciationAmount,
                    e.AccumulatedTotalBalance,
                    e.TotalBookValue
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
            var vouchers = _context.Qry201205depreciationMasterViews.AsQueryable();

            if (startDate.HasValue && endDate.HasValue)
            {

                vouchers = vouchers.Where(v => v.DeprStartDate >= startDate && v.DeprEndDate <= endDate);
            }

            return Ok(vouchers.ToList());
        }

    }
}