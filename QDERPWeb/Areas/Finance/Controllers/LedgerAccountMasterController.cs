
using System.Diagnostics;
using System.Xml.Linq;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;

//using static DevExpress.Xpo.Helpers.AssociatedCollectionCriteriaHelper;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class LedgerAccountMasterController : Controller
	{
		private ERPMasterWtDataContext _context;

		public LedgerAccountMasterController(ERPMasterWtDataContext context)
		{
			_context = context;
		}

		[HttpPost]
		public async Task<ActionResult> SaveLedgerMasterDetails([FromBody] Tbl201AccountGroup AG)
		{
			if (AG == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				_context.Tbl201AccountGroups.Add(AG);
				await _context.SaveChangesAsync();
				//return Json(new { VoucherEntryNo = VE.VoucherNo });
				return Ok(new { success = true, message = "Data inserted successfully!" });
			}
			catch (Exception ex)
			{

				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}


		}

		[HttpGet]
		public async Task<ActionResult> GetAccountMaster(DataSourceLoadOptions loadOptions)
		{

			var qryListOfAccountlists = _context.Tbl201MasterGroups.Select(i => new

			{
				//i.MasterGroupId,
				i.MasterGroup
				//i.MasterGroupAr,
				//i.IsCalculateOpeningBalance,
				//i.IsProfitLossAccount,
				//i.IsBalanceSheetAccount



			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}
		[HttpGet]
		public async Task<ActionResult> GetAccountMasterArabic(DataSourceLoadOptions loadOptions)
		{

			var qryListOfAccountlists = _context.Tbl201MasterGroups.Select(i => new

			{
				//i.MasterGroupId,
				//i.MasterGroup
				i.MasterGroupAr
				//i.IsCalculateOpeningBalance,
				//i.IsProfitLossAccount,
				//i.IsBalanceSheetAccount



			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}



		//[HttpGet]
		//public async Task<ActionResult> GetAccountGroupId(DataSourceLoadOptions loadOptions)
		//{
		//    // Get the voucher No. string and Get the next serial of the voucher No.

		//    string voucherString = "A";
		//    string strNewReceiptNo;

		//    // SQL query to get the max voucher number


		//    string sql = "SELECT MAX(CAST(RIGHT(AccountGroupID, 3) AS INT)) AS MaxAccountGroupID " +
		//                  "FROM tbl201AccountGroups " +
		//                  "WHERE AccountGroupID LIKE {0}";

		//    try
		//    {
		//        var result = await _context.SqlQueryAsync<AccountMasterResult>(sql, new object[] { voucherString + "%" });

		//        int MaxAccountGroupID = result.FirstOrDefault()?.MaxAccountGroupID ?? 0; // Handle null result


		//        int newAccountGroupID = MaxAccountGroupID + 1;

		//        // Format the new voucher number with leading zeros
		//        strNewReceiptNo = "000" + newAccountGroupID.ToString();
		//        strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

		//        // Concatenate with the voucher string
		//        strNewReceiptNo = voucherString + strNewReceiptNo;
		//    }
		//    catch (Exception)
		//    {
		//        // Handle cases where there's no existing voucher number
		//        strNewReceiptNo = voucherString + "102";
		//    }

		//    return Json(strNewReceiptNo);
		//}

		[HttpGet]
		public async Task<ActionResult> GetAccountGroupId(DataSourceLoadOptions loadOptions)
		{
			string voucherString = "A";
			string strNewReceiptNo;

			string likePattern = voucherString + "%";

			try
			{
				// Execute the raw SQL query with interpolated parameters
				var result = await _context.Set<AccountMasterResult>()
					.FromSqlInterpolated($@"
                SELECT MAX(CAST(RIGHT(AccountGroupID, 3) AS INT)) AS MaxAccountGroupID
                FROM tbl201AccountGroups
                WHERE AccountGroupID LIKE {likePattern}")
					.ToListAsync();

				int maxAccountGroupID = result.FirstOrDefault()?.MaxAccountGroupID ?? 0;

				int newAccountGroupID = maxAccountGroupID + 1;

				// Format the new AccountGroupID with leading zeros
				strNewReceiptNo = "000" + newAccountGroupID.ToString();
				strNewReceiptNo = strNewReceiptNo.Substring(strNewReceiptNo.Length - 3);

				strNewReceiptNo = voucherString + strNewReceiptNo;
			}
			catch (Exception)
			{
				// Handle cases where there's no existing AccountGroupID
				strNewReceiptNo = voucherString + "101";
			}

			return Json(strNewReceiptNo);
		}




		[HttpGet]
		public async Task<ActionResult> GetAccountMasterAR(string MasterGroup)
		{
			string val = "";
			try
			{
				if (string.IsNullOrEmpty(MasterGroup))
				{
					return BadRequest("MasterGroup parameter is required.");
				}

				var result = await _context.Tbl201MasterGroups
					.Where(x => x.MasterGroup == MasterGroup)
					.Select(x => x.MasterGroupAr)
					.FirstOrDefaultAsync();
				val = result.ToString();
			}
			catch (ArgumentException argEx)
			{
				// Log the detailed exception for debugging
				Console.WriteLine($"ArgumentException: {argEx.Message}, ParamName: {argEx.ParamName}");
				return BadRequest($"Invalid argument: {argEx.ParamName}");
			}
			catch (Exception ex)
			{
				// Log the general exception
				Console.WriteLine($"Exception: {ex.Message}");
				return StatusCode(500, "An error occurred while processing your request.");
			}
			return Json(val);
		}

		[HttpGet]
		public async Task<ActionResult> GetDocumentType(DataSourceLoadOptions loadOptions)
		{

			var qryListOfAccountlists = _context.Tbl101DocumentTypes.Select(i => new

			{
				i.DocumentTypeId,
				i.DocumentType
				//i.ReminderDays,
				//i.IsEmployeeDocument
			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}


		[HttpGet]
		public async Task<ActionResult> GetPaymentAccounts(DataSourceLoadOptions loadOptions)
		{

			//var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
			var qryListOfAccountlists = _context.Qry201ListOfAccounts.Where(p => p.AccountId != null).Select(i => new
			{
				i.MasterGroupId,
				i.MasterGroup,
				i.AccountGroupId,
				i.AccountId,
				i.AccountHead,
				i.AccountHeadArabic,
				i.ReferenceNo,
				i.IsLedgerObselete

			});

			return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		}

		[HttpGet]
		public async Task<ActionResult> GetEmployeeDetails(DataSourceLoadOptions loadOptions)
		{

			//var qryListOfAccountlists = _context.QryCashAndBankAccounts.Where(p => p.AccountGroupId != null).Select(i => new
			var result = (from balance in _context.Qry20167SalaryLedgerPayableBalances
						  join employee in _context.Tbl101Employees
						  on balance.EmployeeNo equals employee.EmployeeId into empGroup
						  from emp in empGroup.DefaultIfEmpty() // LEFT OUTER JOIN
						  group new { balance, emp } by new { balance.EmployeeNo, balance.EmployeeName, emp.NationalId } into grouped
						  where grouped.Sum(x => x.balance.Balance) < 0
						  select new
						  {
							  EmployeeNo = grouped.Key.EmployeeNo,
							  EmployeeName = grouped.Key.EmployeeName,
							  NationalID = grouped.Key.NationalId
						  });


			return Json(await DataSourceLoader.LoadAsync(result, loadOptions));

		}

		//    [HttpGet]
		//    public async Task<ActionResult> GetDocumentNo(DataSourceLoadOptions loadOptions)
		//    {
		//        // Get the voucher No. string and Get the next serial of the voucher No.


		//        int newAccountGroupID;
		//        string DocumentNo = "";
		//        // SQL query to get the max voucher number


		//        string sql = "SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo " +
		//                      "FROM Tbl20116LedgerDocuments ";

		//        try
		//        {

		//            var results = await _context.VoucherResults
		//.FromSqlInterpolated($"SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo FROM Tbl20116LedgerDocuments")
		//.ToListAsync();

		//            int MaxAccountGroupID = results.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


		//            newAccountGroupID = MaxAccountGroupID + 1;
		//            DocumentNo = newAccountGroupID.ToString();


		//        }
		//        catch (Exception)
		//        {
		//            // Handle cases where there's no existing voucher number
		//            DocumentNo = "1";
		//        }

		//        return Json(DocumentNo);
		//    }

		//    [HttpPost]
		//    public async Task<ActionResult> AddDocumentsEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument documentdetails,string DocumentType)
		//    {
		//        if (documentdetails == null)
		//        {
		//            return BadRequest(new { success = false, message = "Invalid data received." });
		//        }
		//        int newAccountGroupID;
		//        string DocumentNo = "";

		//        try
		//        {
		//            //var PaymentAccount = "Select AccountHead From tbl201ChartOfAccounts where AccountGroupID = 'A012'and AccountHead = ''";

		//            // Add entries to the database

		//            var results = await _context.VoucherResults
		//.FromSqlInterpolated($"SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo FROM Tbl20116LedgerDocuments")
		//.ToListAsync();

		//            int MaxAccountGroupID = results.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


		//            newAccountGroupID = MaxAccountGroupID + 1;
		//            documentdetails.DocumentNo = newAccountGroupID.ToString();
		//            _context.Tbl20116LedgerDocuments.Add(documentdetails);


		//            await _context.SaveChangesAsync();
		//            var qryListOfAccountlists = _context.Tbl20116LedgerDocuments.Where(p => p.DocumentNo == documentdetails.DocumentNo).Select(i => new
		//            {
		//                i.DocumentNo,
		//                //i.DocumentType,
		//                i.DocumentRefNo,
		//                DocumentType,
		//                i.DocumentRemarks,
		//                i.DocumentExpDate,
		//                i.DocumentExpDateAr,
		//                i.NotifiedOn,
		//            });

		//            return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		//        }
		//        catch (Exception ex)
		//        {
		//            return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//        }
		//    }

		[HttpPost]
		public async Task<ActionResult> AddDocumentsEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument documentdetails, string DocumentType)
		{
			if (documentdetails == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				// Generate a new document number.
				var newDocumentNo = await GenerateDocumentNoAsync();
				documentdetails.DocumentNo = newDocumentNo;

				// Add the new document entry to the database.
				_context.Tbl20116LedgerDocuments.Add(documentdetails);
				await _context.SaveChangesAsync();

				// Query to fetch and return the newly added document details.
				var qryListOfAccountlists = _context.Tbl20116LedgerDocuments
					.Where(p => p.DocumentRefNo == documentdetails.DocumentRefNo)
					.Select(i => new
					{
						i.DocumentNo,
						//i.DocumentType,
						i.DocumentRefNo,
						DocumentType,
						i.DocumentRemarks,
						i.DocumentExpDate,
						i.DocumentExpDateAr,
						i.NotifiedOn,
					});

				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
			}
		}


		[HttpPost]
		public async Task<IActionResult> UpdateDocument(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument updatedDocument)
		{
			try
			{


				if (updatedDocument == null)
				{
					return BadRequest("Invalid document data.");
				}


				//  Find the existing document by DocumentNo
				var document = await _context.Tbl20116LedgerDocuments
					.FirstOrDefaultAsync(d => d.DocumentNo == updatedDocument.DocumentNo);

				//if (document == null)
				//{
				//    return NotFound("Document not found.");
				//}

				//// Update the fields
				//document.DocumentType = updatedDocument.DocumentType;
				//document.DocumentRefNo = updatedDocument.DocumentRefNo;
				//document.DocumentRemarks = updatedDocument.DocumentRemarks;
				//document.DocumentExpDate = updatedDocument.DocumentExpDate;
				//document.DocumentExpDateAr = updatedDocument.DocumentExpDateAr;
				//document.NotifiedOn = updatedDocument.NotifiedOn;

				//// Save changes to the database
				//await _context.SaveChangesAsync();

				var qryListOfAccountlists = _context.Tbl20116LedgerDocuments.Where(p => p.DocumentNo == document.DocumentNo).Select(i => new
				{
					i.DocumentNo,
					i.DocumentType,
					i.DocumentRefNo,
					i.DocumentRemarks,
					i.DocumentExpDate,
					i.DocumentExpDateAr,
					i.NotifiedOn,
				});


				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
			}
			catch (Exception ex)
			{
				throw ex;
			}



		}

		//[HttpPost]
		//public async Task<ActionResult> AddEmployeeEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl20114SalaryPayableMaster salarydetails, string EmployeeName)
		//{
		//    if (salarydetails == null)
		//    {
		//        return BadRequest(new { success = false, message = "Invalid data received." });
		//    }

		//    try
		//    {

		//        // Add entries to the database
		//        _context.Tbl20114SalaryPayableMasters.Add(salarydetails);

		//        await _context.SaveChangesAsync();

		//        // Add the WHERE condition for ReferenceNo
		//        var qryListOfAccountlists = _context.Tbl20114SalaryPayableMasters
		//            .Where(p => p.EmployeeNo == salarydetails.EmployeeNo && p.ReferenceNo == salarydetails.ReferenceNo && p.ReferenceType == salarydetails.ReferenceType && p.Amount == salarydetails.Amount && p.DrCr == salarydetails.DrCr)
		//            .Select(i => new
		//            {
		//                i.SalaryPayableLedgerNo,
		//                i.ReferenceType,
		//                i.ReferenceNo,
		//                i.EmployeeNo,
		//                EmployeeName,
		//                i.Amount,
		//                i.DrCr
		//            });

		//        return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
		//    }
		//    catch (Exception ex)
		//    {
		//        return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
		//    }
		//}

		[HttpPost]
		public async Task<ActionResult> AddEmployeeEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl20114SalaryPayableMaster salarydetails, string EmployeeName)
		{
			if (salarydetails == null)
			{
				return BadRequest(new { success = false, message = "Invalid data received." });
			}

			try
			{
				// Validate EmployeeNo
				var employeeInfo = _context.Qry20167SalaryLedgerPayableBalances
					.Where(s => s.EmployeeNo == EmployeeName)
					.Select(s => new
					{
						s.EmployeeNo,
						s.EmployeeName,
						ReferenceNo = s.ReferenceNo
					})
					.FirstOrDefault();

				if (employeeInfo == null)
				{
					return NotFound(new { success = false, message = "Employee not found in the ledger balances." });
				}

				// Map the employee details
				salarydetails.EmployeeNo = employeeInfo.EmployeeNo;
				salarydetails.ReferenceNo = employeeInfo.ReferenceNo;

				// Add entries to the database
				_context.Tbl20114SalaryPayableMasters.Add(salarydetails);

				// Save changes
				await _context.SaveChangesAsync();

				// Query the updated list
				var qryListOfAccountlists = _context.Tbl20114SalaryPayableMasters
					.Where(p => p.EmployeeNo == salarydetails.EmployeeNo
								&& p.ReferenceNo == salarydetails.ReferenceNo
								&& p.ReferenceType == salarydetails.ReferenceType
								&& p.Amount == salarydetails.Amount
								&& p.DrCr == salarydetails.DrCr)
					.Select(i => new
					{
						i.SalaryPayableLedgerNo,
						i.ReferenceType,
						i.ReferenceNo,
						i.EmployeeNo,
						EmployeeName = employeeInfo.EmployeeName, // Use the queried EmployeeName
						i.Amount,
						i.DrCr
					});

				return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
			}
		}



		[HttpGet]
		public async Task<IActionResult> GetAgainstPayable([FromQuery] string inputParameter)
		{
			if (string.IsNullOrEmpty(inputParameter))
			{
				Console.WriteLine("Error: inputParameter is null or empty.");
				return BadRequest("inputParameter cannot be null or empty");
			}
			try
			{
				var result = _context.Qry20167SalaryLedgerPayableBalances
					.Where(s => s.EmployeeNo == inputParameter)
					.Select(s => new
					{
						s.EmployeeNo,
						s.EmployeeName,
						s.ReferenceNo,
						s.PayableAmount,
						s.Paid,
						s.Balance
					});
				return Json(result);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}



		[HttpPost]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			try
			{
				if (file == null || file.Length == 0)
				{
					return BadRequest("No file uploaded.");
				}

				// Use a configurable path for storing uploads
				var uploadsFolder = @"D:\!QuickDiceDMS-\VoucherScanned"; // Updated the path here
				if (!Directory.Exists(uploadsFolder))
				{
					Directory.CreateDirectory(uploadsFolder); // Ensure the directory exists
				}

				// Sanitize file name
				var fileName = Path.GetFileName(file.FileName);

				// Check if the file already exists
				var filePath = Path.Combine(uploadsFolder, fileName);
				if (System.IO.File.Exists(filePath))
				{
					return Conflict(new { Message = "File with the same name already exists." });
				}

				// Save the file
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return Ok(new { Message = "File uploaded successfully.", FilePath = filePath });
			}
			catch (Exception ex)
			{
				// Log the exception (using a logger is recommended)
				return StatusCode(500, new { Message = "An error occurred while uploading the file.", Details = ex.Message });
			}
		}


		[HttpPost]
		public IActionResult OpenFileExplorer(string documentNo)
		{
			try
			{
				documentNo = "0";
				// Construct the path
				var basePath = @"D:\!QuickDiceDMS-\VoucherScanned";
				var filePath = System.IO.Path.Combine(basePath);

				// Open the file explorer at the specified path
				Process.Start(new ProcessStartInfo
				{
					FileName = "explorer.exe",
					Arguments = filePath,
					UseShellExecute = true
				});

				return Json(new { success = true });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, message = ex.Message });
			}
		}

		private async Task<string> GenerateDocumentNoAsync()
		{
			string documentNo = "1"; // Default value if no records exist.
			int newAccountGroupID;

			try
			{
				// Query to get the maximum document number.

				var results = await _context.VoucherResults
	.FromSqlInterpolated($"SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo FROM Tbl20116LedgerDocuments")
	.ToListAsync();

				int MaxAccountGroupID = results.FirstOrDefault()?.MaxVoucherNo ?? 0; // Handle null result


				newAccountGroupID = MaxAccountGroupID + 1;
				documentNo = newAccountGroupID.ToString();

			}
			catch
			{
				// Handle any potential errors by using default "1".
				documentNo = "1";
			}

			return documentNo;
		}

		[HttpGet]
		public async Task<ActionResult> GetDocumentNo(DataSourceLoadOptions loadOptions)
		{
			var documentNo = await GenerateDocumentNoAsync();
			return Json(documentNo);
		}
	}
}
