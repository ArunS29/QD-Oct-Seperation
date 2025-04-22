
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
        [HttpPut]
        public async Task<ActionResult> UpdateLedgerMasterDetails([FromBody] Tbl201AccountGroup AG)
        {
            if (AG == null || AG.AccountGroupId == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                var existingRecord = await _context.Tbl201AccountGroups.FindAsync(AG.AccountGroupId);

                if (existingRecord == null)
                {
                    return NotFound(new { success = false, message = "Account Group not found." });
                }

                // Update existing fields
                existingRecord.AccountGroup = AG.AccountGroup;
              
       
                existingRecord.AccountGroupAr = AG.AccountGroupAr;

                existingRecord.AccountGroupUnder = AG.AccountGroupUnder ?? existingRecord.AccountGroupUnder;

                existingRecord.RecordCreatedBy = AG.RecordCreatedBy;
                existingRecord.RecordCreatedOn = AG.RecordCreatedOn;
                existingRecord.RecordModifiedBy = AG.RecordModifiedBy;
                existingRecord.RecordModifiedOn = AG.RecordModifiedOn;

                _context.Tbl201AccountGroups.Update(existingRecord);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Data updated successfully!" });
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
                i.MasterGroupId,
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
                i.MasterGroupId,
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
            try
            {
                if (string.IsNullOrEmpty(MasterGroup))
                    return BadRequest("MasterGroup parameter is required.");

                var result = await _context.Tbl201MasterGroups
                    .Where(x => x.MasterGroup == MasterGroup)
                    .Select(x => new {
                        x.MasterGroupId,
                        x.MasterGroupAr
                    })
                    .FirstOrDefaultAsync();

                if (result == null)
                    return NotFound("No matching record found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpGet]
        public async Task<ActionResult> GetDocumentType(DataSourceLoadOptions loadOptions)
        {

            var qryListOfAccountlists = _context.Tbl101DocumentTypes.Select(i => new

            {
                i.DocumentTypeId,
                i.DocumentType

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
        public async Task<ActionResult> AddAssetDocumentsEntry(DataSourceLoadOptions loadOptions, [FromBody] Tbl20108AssetDocument documentdetails, string DocumentType)
        {
            const string voucherPrefix = "L00"; // Constant for the voucher prefix
            if (documentdetails == null)
            {
                return BadRequest(new { success = false, message = "Invalid data received." });
            }

            try
            {
                // Generate a new document number.
                var newDocumentNo = await GenerateAssetDocumentNoAsync();
                documentdetails.DocumentNo = newDocumentNo;

                // SQL query with interpolated string to fetch the max voucher number
                var result = await _context.VoucherResults
      .FromSqlInterpolated($@"
        SELECT CAST(RIGHT(AssetLedgerNo, 3) AS INT) AS MaxVoucherNo
        FROM Tbl20108AssetDocuments
        WHERE AssetLedgerNo LIKE {voucherPrefix + "%"}
        ORDER BY MaxVoucherNo DESC
    ")
      .ToListAsync();

                // Get the max voucher number or default to 0 if no records are found
                int maxVoucherNo = result.FirstOrDefault()?.MaxVoucherNo ?? 0;
                int newVoucherNo = maxVoucherNo + 1;

                // Format the new voucher number with leading zeros
                //    var formattedVoucherNo = newVoucherNo.ToString("D3"); // Ensures 3 digits
                documentdetails.AssetLedgerNo = voucherPrefix + newVoucherNo;

                // Add the new document entry to the database
                _context.Tbl20108AssetDocuments.Add(documentdetails);
                await _context.SaveChangesAsync();

                var qryListOfAccountlists = _context.Tbl20108AssetDocuments
          .Where(p => p.DocumentRefNo == documentdetails.DocumentRefNo)
          .Join(
              _context.Tbl101DocumentTypes,
              document => document.DocumentType,
              docType => docType.DocumentTypeId,
              (document, docType) => new
              {
                  document.DocumentNo,
                  document.DocumentRefNo,
                  docType.DocumentType,
                  document.DocumentRemarks,
                  document.DocumentExpDate,
                  document.DocumentExpDateAr,
                  document.DocumentNotificationDate
              });


                // Return the data using DataSourceLoader
                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }
            catch (Exception ex)
            {
                // Improved error response with more detailed error handling
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}", details = ex.StackTrace });
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

        [HttpPost]
        public async Task<IActionResult> UpdateAssetDocument(DataSourceLoadOptions loadOptions, [FromBody] Tbl20116LedgerDocument updatedDocument)
        {
            try
            {
                if (updatedDocument == null)
                {
                    return BadRequest("Invalid document data.");
                }

                // Find the existing document by DocumentNo
                var document = await _context.Tbl20108AssetDocuments
                    .FirstOrDefaultAsync(d => d.DocumentNo == updatedDocument.DocumentNo);

                // Check if the document exists
                if (document == null)
                {
                    return NotFound($"Document with DocumentNo {updatedDocument.DocumentNo} not found.");
                }

                // Query the list of account lists
                var qryListOfAccountlists = _context.Tbl20108AssetDocuments
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

                // Return the data
                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "An error occurred while processing your request.");
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
        private async Task<string> GenerateAssetDocumentNoAsync()
        {
            string documentNo = "1"; // Default value if no records exist.
            int newAccountGroupID;

            try
            {
                // Query to get the maximum document number.

                var results = await _context.VoucherResults
    .FromSqlInterpolated($"SELECT MAX(CAST(RIGHT(DocumentNo, 3) AS INT)) AS MaxDocumentNo FROM Tbl20108AssetDocuments")
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
        public async Task<ActionResult> GetAssetDocumentNo(DataSourceLoadOptions loadOptions)
        {
            var documentNo = await GenerateAssetDocumentNoAsync();
            return Json(documentNo);
        }
        [HttpGet]
        public IActionResult GetAccountLedgerUnder(string AccountGroupUnder)
        {
            try
            {
                var masterGroup = _context.Tbl201MasterGroups
                                .Where(m => m.MasterGroupId == AccountGroupUnder)
                                .Select(m => m.MasterGroup)
                                .FirstOrDefault();

                if (masterGroup == null)
                {
                    return NotFound("MasterGroup not found.");
                }

                return Json(new { masterGroup });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            // LINQ query to fetch MasterGroup


        }

        [HttpGet]
        public IActionResult GetAccountGroupUnderID(string id)
        {
            try
            {

                // Query the database for the specified AccountGroupID
                var accountGroupUnder = _context.Tbl201AccountGroups
                    .Where(ag => ag.AccountGroupId == id)
                    .Select(ag => ag.AccountGroupUnder)
                    .FirstOrDefaultAsync();

                if (accountGroupUnder == null)
                {
                    return NotFound("Account group not found.");
                }
                return Json(new { accountGroupUnder.Result });

            }
            catch (Exception ex)
            {
                throw ex;
            }
            // LINQ query to fetch MasterGroup


        }
        [HttpPost]
        public async Task<ActionResult> DeleteLedgerMasterDetails(string accountGroupId)
        {
            // Look up the record in your DbContext
            var record = await _context.Tbl201AccountGroups.FindAsync(accountGroupId);
            if (record == null)
            {
                return NotFound(new { success = false, message = "Record not found." });
            }

            try
            {
                _context.Tbl201AccountGroups.Remove(record);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Record deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetAccountGroups()
        {
            try
            {
                var accountGroups = _context.Tbl201AccountGroups
                    .Select(a => new
                    {
                        a.AccountGroupId,
                        a.AccountGroup
                    })
                    .ToList();

                return Ok(accountGroups);
            }
            catch (Exception ex)
            {
                // Log the exception (Assuming you have logging configured)
                Console.WriteLine($"Error fetching account groups: {ex.Message}");

                // Return a 500 Internal Server Error response with error details
                return StatusCode(500, new { message = "An error occurred while retrieving account groups.", error = ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetAccountGroupsData(string AccountGroupId)
        {
            try
            {
                var accountGroup = _context.Tbl201AccountGroups
                    .Where(a => a.AccountGroupId == AccountGroupId)
                    .Select(a => new
                    {
                        a.AccountGroupId,
                        a.AccountGroup,
                        a.AccountGroupAr,
                        a.RecordCreatedBy,
                        a.RecordCreatedOn,
                        a.IsUseInSales,
                        a.IsUsedInPurchase,
                        a.IsUseInReconciliation,
                        a.IsSalaryPayable
                    })
                    .FirstOrDefault();

                if (accountGroup == null)
                {
                    return NotFound(new { success = false, message = "Account Group not found." });
                }

                return Ok(accountGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving data", error = ex.Message });
            }
        }

    }
}
