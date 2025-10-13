using QD.ERP.Shared.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.Service;
using System.Drawing;
using System.Xml.Linq;
using DevExpress.Xpo;
using System.Data;
using QD.ERP.Shared.Models;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SalesVoucherController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<SalesVoucherController> _logger;

        private readonly FcmService _fcmService;

        public SalesVoucherController(ILogger<SalesVoucherController> logger, TenantDbContextHelper tenantDbContextHelper, FcmService fcmService)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _fcmService = fcmService;
        }

        [HttpGet]
        public async Task<ActionResult> GetClientName(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
					.Where(p => p.AccountGroupId == "A011" || p.AccountGroupId == "A012") // Applying the filter
					.Select(i => new
				
                    {
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.AccountGroup,
                        i.AccountGroupId,
                        i.AccountId,
                        i.AccountHead,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.IsLedgerObselete
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet]
        public async Task<ActionResult> GetAccountHead(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
				   .Where(i => i.IsUseInSales == true ) // Applying the filter// Applying the filter
					.Select(i => new
                    {
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.AccountGroup,
                        i.AccountGroupId,
                        i.AccountId,
                        i.AccountHead,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.IsLedgerObselete,
                        i.IsUseInSales,
                        i.IsUsedInPurchase
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



		[HttpPost]
		public async Task<ActionResult> AddVoucherEntry(DataSourceLoadOptions loadOptions, [FromBody] List<Tbl201VoucherEntry> voucherEntries, string AccountHead, string PaymentAccoutHeadName, int Gridcount)
		{
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                return Unauthorized(new { success = false, message = "Invalid tenant." });
            }
				if (voucherEntries == null || !voucherEntries.Any())
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				Tbl201VoucherMaster voucherMaster = new();
				int aEntryAmount = 0;
				int Amt = 0;
				var Remarks = "";
				bool IsMatchingEntry = false;
				//var matchingEntries;
				List<VoucherEntryDisplayDTO> matchingEntries = new();
				var voucherNos1 = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
				var qryListOfAccountlists1 = dbContext.Qry201VoucherEntryScreenDisplays
					.Where(p => voucherNos1.Contains(p.VoucherNo))
					.OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
					.Select(i => new VoucherEntryDisplayDTO
					{
						VoucherNo = i.VoucherNo,
						VoucherEntryNo = i.VoucherEntryNo,
						DrCr = i.DrCr,
						DrAmount = i.DrAmount,
						CrAmount = i.CrAmount,
						EntryNarration = i.EntryNarration,
						AccountHead = i.AccountHead,
						SysRemarks = i.SysRemarks
					});

				var resultList1 = await qryListOfAccountlists1.ToListAsync();
				int crCount1 = resultList1.Count(i => i.DrCr == "Dr");
				if (crCount1 >= 2)
				{
					matchingEntries = resultList1.Where(x => x.AccountHead == PaymentAccoutHeadName).ToList();
					IsMatchingEntry = true;
				}

				bool isVoucherExists = dbContext.Tbl201VoucherMasters
			   .Any(v => v.VoucherNo == voucherEntries[0].VoucherNo);

				if (!isVoucherExists)
				{
					voucherMaster.VoucherNo = voucherEntries[0].VoucherNo;
					voucherMaster.VoucherDate = DateTime.Now;
					dbContext.Tbl201VoucherMasters.AddRange(voucherMaster);
				}

				// Add entries to the database
				dbContext.Tbl201VoucherEntries.AddRange(voucherEntries);
				await dbContext.SaveChangesAsync();

				//SaveVoucher(voucherEntries);


				var voucherNos = voucherEntries.Select(ve => ve.VoucherNo).Distinct();
				var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
					.Where(p => voucherNos.Contains(p.VoucherNo))
					.OrderBy(i => i.DrCr == "Dr" ? 1 : 0) // Ensures "Dr" entries come first
					.Select(i => new VoucherEntryDisplayDTO
					{
						VoucherNo = i.VoucherNo,
						VoucherEntryNo = i.VoucherEntryNo,
						DrCr = i.DrCr,
						DrAmount = i.DrAmount,
						CrAmount = i.CrAmount,
						EntryNarration = i.EntryNarration,
						AccountHead = i.AccountHead,
						SysRemarks = i.SysRemarks
					});

				var resultList = await qryListOfAccountlists.ToListAsync();

				int debitamt = 0; // Initialize debit amount

				int crCount = resultList.Count(i => i.DrCr == "Dr");

				//var lastCrEntry = resultList.LastOrDefault(i => i.DrCr == "Cr");

				var newDrEntry = resultList
	.Where(i => i.DrCr == "Dr")
	.OrderByDescending(i => i.VoucherEntryNo) // Assuming this is sequential
	.FirstOrDefault();



				var newlist = newDrEntry != null ? new List<VoucherEntryDisplayDTO> { newDrEntry } : new List<VoucherEntryDisplayDTO>();

				var pettycashid = "";
				foreach (var petty in newlist)
				{
					pettycashid = petty.AccountHead;
				}

				foreach (var entry in resultList)
				{


					if (!string.IsNullOrEmpty(entry.AccountHead))
					{

						var accountHead = dbContext.Qry201ListOfAccounts
												  .Where(a => a.AccountId == entry.AccountHead)
												  .Select(a => a.AccountHead)
												  .FirstOrDefault();



						if (Gridcount != 0)
						{
							// Calculate Debit Amount (DrAmount)

							debitamt = (int)(debitamt + entry.CrAmount);

							// If Dr/Cr is Credit ("Cr"), perform specific logic
							if (entry.DrCr == "Dr" && crCount == 1)
							// if (crCount==1)
							{
								// Check if an existing entry matches
								var existingEntry = dbContext.Tbl201VoucherEntries
															.FirstOrDefault(v => v.AccountHead == entry.AccountHead
																			  && v.DrCr == "Dr"
																			  && v.VoucherNo == entry.VoucherNo);

								if (existingEntry != null)
								{
									// Update CrAmount by adding the calculated debit amount
									entry.DrAmount = debitamt;
									entry.SysRemarks = Remarks;

									// Optionally update the existing entry in the database
									existingEntry.VoucherAmount = entry.DrAmount;
									existingEntry.SysRemarks = entry.SysRemarks;
									dbContext.Tbl201VoucherEntries.Update(existingEntry);
									dbContext.SaveChanges();

								}
								else
								{
									// If no existing entry, assign CrAmount as debitamt
									entry.DrAmount = debitamt;
								}
							}
							else if (newDrEntry != null && crCount >= 2 && entry.AccountHead == pettycashid)
							{
								if (entry.DrCr == "Dr")
								{

									if (IsMatchingEntry == true)
									{
										// Accumulate `aEntryAmount` correctly
										foreach (var mEntry in matchingEntries)
										{
											aEntryAmount += (int)mEntry.DrAmount; // Accumulate CrAmount correctly
										}

										// Process voucherEntries
										foreach (var mVoucherEntry in voucherEntries)
										{
											int eAmount = (int)mVoucherEntry.VoucherAmount;
											Amt = aEntryAmount + eAmount;

											// Do something with Amt if required
										}

										// Check if an existing entry matches
										var existingEntry = dbContext.Tbl201VoucherEntries
																	.FirstOrDefault(v => v.AccountHead == entry.AccountHead
																					  && v.DrCr == "Dr"
																					  && v.VoucherNo == entry.VoucherNo);

										if (existingEntry != null)
										{
											// Update CrAmount by adding the calculated debit amount
											entry.DrAmount = Amt;
											entry.SysRemarks = Remarks;

											// Optionally update the existing entry in the database
											existingEntry.VoucherAmount = entry.DrAmount;
											existingEntry.SysRemarks = entry.SysRemarks;
												dbContext.Tbl201VoucherEntries.Update(existingEntry);
												dbContext.SaveChanges();

										}
										else
										{
											// If no existing entry, assign CrAmount as debitamt
											// entry.CrAmount = debitamt;
										}
									}

								}

							}

						}
						entry.AccountHead = accountHead;
						if (Remarks == "")
						{
							Remarks = entry.AccountHead;
						}
						else
						{
							Remarks = Remarks + "," + entry.AccountHead;
						}
					}

				}

				return Json(DataSourceLoader.Load(resultList.AsQueryable(), loadOptions));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}
		
		}

		[HttpGet]
        public async Task<ActionResult> GetVoucherEntries(DataSourceLoadOptions loadOptions, string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201VoucherEntryScreenDisplays
                    .Where(p => p.VoucherNo == voucherNo)
                    .Select(i => new
                    {
                        i.VoucherNo,
                        i.DrCr,
                        i.DrAmount,
                        i.CrAmount,
                        i.EntryNarration,
                        i.AccountHead,
                        i.SysRemarks,
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<ActionResult> SaveVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            if (VM == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    dbContext.Tbl201VoucherMasters.Add(VM);
                    await dbContext.SaveChangesAsync();

                     var UserId = HttpContext.Session.GetString("UserId");
                    var TenantName = HttpContext.Session.GetString("TenantName");

                    var notifyRequest = new NotificationRequest
                        {
                             UserId = UserId, // or fetch from session/DB
                             VoucherName = "Sales Voucher",
                             ActionType = "You have one Sales Voucher to verify",
                             TenantName = TenantName 
                    };

                await _fcmService.SendNotificationAsync(notifyRequest);

                    return Ok(new { success = true, message = "Data inserted successfully!" });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in SaveVoucher: {ex.Message}");
                    return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
		[HttpGet]
		public async Task<ActionResult> GetVoucherDetails(DataSourceLoadOptions loadOptions, string voucherNo)
		{
			if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
			{
				if (string.IsNullOrEmpty(voucherNo))
				{
					return BadRequest(new { success = false, message = "Invalid data received." });
				}
				try
				{
					var voucherDetails = dbContext.Tbl201VoucherMasters
						.Where(p => p.VoucherNo.ToLower() == voucherNo.ToLower());

					if (!voucherDetails.Any())
					{
						return NotFound(new { success = false, message = "Voucher not found." });
					}

					var result = await DataSourceLoader.LoadAsync(voucherDetails, loadOptions);
					return Json(result);
				}
				catch (Exception ex)
				{
					_logger.LogError($"Error in GetVoucherDetails: {ex.Message}");
					return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
				}
			}

			return Unauthorized(new { message = "Invalid tenant.", success = false });
		}

		[HttpGet]
        public IActionResult CheckVoucherExists(string voucherNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                bool exists = dbContext.Tbl201VoucherEntries.Any(s => s.VoucherNo == voucherNo);

                if (exists)
                {
                    return Ok(new { success = false, message = $"The entered Voucher No has already been recorded in the system. Please correct the Voucher No." });
                }

                return Ok(new { success = true });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssetDocument(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument updatedDocument)
        {
            if (updatedDocument == null)
            {
                return BadRequest("Invalid document data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var document = await dbContext.Tbl20108AssetDocuments
                        .FirstOrDefaultAsync(d => d.DocumentNo == updatedDocument.DocumentNo);

                    if (document == null)
                    {
                        return NotFound($"Document with DocumentNo {updatedDocument.DocumentNo} not found.");
                    }

                    var qryListOfAccountlists = dbContext.Tbl20108AssetDocuments
                        .Where(p => p.DocumentNo == document.DocumentNo)
                        .Select(i => new
                        {
                            i.DocumentNo,
                            i.DocumentType,
                            i.DocumentRefNo,
                            i.DocumentRemarks,
                            i.DocumentExpDate,
                            i.DocumentExpDateAr,
                            i.DocumentNotificationDate,
                        });

                    return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in UpdateAssetDocument: {ex.Message}");
                    return StatusCode(500, "An error occurred while processing your request.");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //[HttpPost]
        //public async Task<ActionResult> UpdateSalesVoucher([FromBody] Tbl201VoucherMaster VM)
        //{
        //    if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
        //    {
        //        return BadRequest(new { success = false, message = "Invalid data received." });
        //    }

        //    try
        //    {

        //        if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //        {
        //            var existingVoucher = await dbContext.Tbl201VoucherMasters
        //                                            .FirstOrDefaultAsync(v => v.VoucherNo == VM.VoucherNo);

        //            if (existingVoucher != null)
        //            {
        //                // Update existing record
        //                dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);
        //            }
        //            else
        //            {
        //                // Insert new record
        //                dbContext.Tbl201VoucherMasters.Add(VM);
        //            }

        //            await dbContext.SaveChangesAsync();
        //            return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError($"Error in UpdateAssetDocument: {ex.Message}");
        //        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
        //    }
        //    return Unauthorized(new { message = "Invalid tenant.", success = false });

        //}


         [HttpPost]
        public async Task<ActionResult> UpdateSalesVoucher([FromBody] Tbl201VoucherMaster VM)
        {
            
            if (VM == null || string.IsNullOrWhiteSpace(VM.VoucherNo))
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var voucherNo = VM.VoucherNo?.Trim(); // Remove whitespace and prevent null issues
                    if (string.IsNullOrEmpty(voucherNo))
                    {
                        return BadRequest(new { success = false, message = "Voucher number is required." });
                    }

                    var existingVoucher = await dbContext.Tbl201VoucherMasters
                                                         .FirstOrDefaultAsync(v => v.VoucherNo == voucherNo);

                    if (existingVoucher != null)
                    {
                        dbContext.Entry(existingVoucher).CurrentValues.SetValues(VM);


                        //dbContext.Tbl201VoucherMasters.Update(existingVoucher);
                    }
                    else
                    {
                        // Insert new record
                        dbContext.Tbl201VoucherMasters.Add(VM);
                    }

                    await dbContext.SaveChangesAsync();
                    return Ok(new { success = true, message = existingVoucher != null ? "Voucher updated successfully!" : "Voucher inserted successfully!" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateSalesVoucher: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        //[HttpGet]
        //public async Task<IActionResult> CheckVoucherDateLocking(DateTime voucherDate)
        //{
        //	if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
        //	{
        //		bool isDateBlocked = await dbContext.Tbl90117VoucherDateLockings
        //			.AnyAsync(v => v.VoucherTypeCode == "SALES_VOUCHER" && v.VoucherDateLocked >= voucherDate);

        //		if (isDateBlocked)
        //		{
        //			return Json(new { success = false, message = "This Voucher Entry date has been blocked. Please review your entry date." });
        //		}

        //		return Json(new { success = true, todayDate = DateTime.Now.ToString("yyyy-MM-dd") });
        //	}

        //	return Unauthorized(new { message = "Invalid tenant.", success = false });
        //}


        [HttpGet]
        public async Task<IActionResult> CheckVoucherDateLocking(DateTime? voucherDate)
        {
            if (!voucherDate.HasValue)
            {
                return BadRequest(new { success = false, message = "Voucher date is required." });
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                DateTime dateToCheck = voucherDate.Value; // Ensuring it's not null

                var lockedDates = dbContext.Tbl90117VoucherDateLockings
          .Where(v => v.VoucherTypeCode == "SALES_VOUCHER" && v.VoucherDateLocked <= voucherDate)
          .Select(v => v.VoucherDateLocked) // Select only the VoucherDateLocked column
          .ToList();

                // Check in-memory to avoid EF errors
                bool isLocked = lockedDates.Any(lockedDate => dateToCheck <= lockedDate);

                if (isLocked)
                {
                    return Json(new { success = false, message = "This Voucher Entry date has been blocked. Please review your entry date." });
                }

                return Json(new { success = true, todayDate = DateTime.Now.ToString("yyyy-MM-dd") });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

		




	}
}
