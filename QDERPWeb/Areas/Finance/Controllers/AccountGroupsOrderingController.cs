using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QD.ERP.Web.DAL.Entities;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class AccountGroupsOrderingController : Controller
    {
        private ERPMasterWtDataContext _context;
        public AccountGroupsOrderingController(ERPMasterWtDataContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetAccountGroupsOrdering(DataSourceLoadOptions loadOptions)
        {
            // Fetching the data and sorting by AccountGroupOrderNo in ascending order
            var qry20164salarypayableledgermaster = _context.Qry201207accountGroupOrderings
                .Select(i => new
                {
                    i.ChartOfAccountsOrder,
                    i.MasterGroup,
                    i.AccountGroupId,
                    i.AccountGroup,
                    i.AccountGroupOrderNo
                })
                .OrderBy(i => i.AccountGroupOrderNo);  // Sorting by AccountGroupOrderNo in ascending order

            // Returning the result to the client using DataSourceLoader to handle paging, filtering, etc.
            return Json(await DataSourceLoader.LoadAsync(qry20164salarypayableledgermaster, loadOptions));
        }

        [HttpPost]
        public IActionResult UpdateAccountGroupOrder([FromBody] List<Qry201207accountGroupOrdering> updatedData)
        {
            if (updatedData == null || updatedData.Count == 0)
            {
                return BadRequest("No data received");
            }

            foreach (var item in updatedData)
            {
                var existingRecord = _context.Tbl201AccountGroups
                    .FirstOrDefault(x => x.AccountGroupId == item.AccountGroupId);

                if (existingRecord != null)
                {

                    existingRecord.AccountGroupOrderNo = item.AccountGroupOrderNo;
                    _context.Tbl201AccountGroups.Update(existingRecord);
                }


            }

            _context.SaveChanges(); // Save changes to the database

            return Ok(new { message = "Data updated successfully" });
        }

    }
}
