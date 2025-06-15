using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Reports;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;
using Microsoft.Data.SqlClient;

namespace QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements
{
    public partial class BillsReceivableRentation : XtraReport
    {
        

        private readonly TenantDbContextHelper _tenantDbContextHelper;


        public BillsReceivableRentation(string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            LoadCurrencySymbolAndImage();

            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
}
        }

        public BillsReceivableRentation()
  {
    InitializeComponent();
   }

private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
{
    accountId ??= "";
    frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
    toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

    AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
    AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
    AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
    AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
    AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
    AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
    AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
    AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
    AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

    if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
        tenantLabel.Text = tenantName;

    if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
        userNameLabel.Text = username;

    if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
        companyNameLabel.Text = companyName;

    if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
        addressLabel.Text = companyAddress;

    if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
        logoPictureBox.Image = logoImage;

    if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
        companyNameArLabel.Text = companyNameAr;

    if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
        addressArbLabel.Text = companyAddressArb;

    ConfigureDataSource(accountId, frmDate, toDate);
}

private void AddOrUpdateParameter(string paramName, object paramValue, Type paramType, bool visible)
{
    var parameter = Parameters[paramName];
    if (parameter == null)
    {
        Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
        {
            Name = paramName,
            Type = paramType,
            Value = paramValue,
            Visible = visible
        });
    }
    else
    {
        parameter.Value = paramValue;
        parameter.Visible = visible;
    }
}

private void ConfigureDataSource(string accountId, DateTime frmDate, DateTime toDate)
{
    sqlDataSource1.Queries.Clear();

    if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
    {
        sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

        string querySql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                                WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                                AND VoucherDate BETWEEN @StartDate AND @EndDate";

        var customQuery = new CustomSqlQuery
        {
            Name = "qry201SubLedgerReceivablesMaster",
            Sql = querySql
        };

        customQuery.Parameters.AddRange(new[]
        {
                new QueryParameter("@AccountID", typeof(string), accountId),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate)
            });

        sqlDataSource1.Queries.Add(customQuery);
        sqlDataSource1.Name = "sqlDataSource1";
    }
    else
    {
        throw new Exception("Unable to get tenant context. Please check session and cache.");
    }
}

private void BillsReceivableRentation_BeforePrint(object sender, CancelEventArgs e)
        {

        }
        private void LoadCurrencySymbolAndImage()
        {
            try
            {
                if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string connectionString = tenant.ConnectionString;
                string svgText = null;
                string currencySymbol = null;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"SELECT TOP 1 CurrencyImage, CurrencySymbol FROM {tenant.schemaname}.tbl901companyDetails";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                svgText = reader["CurrencyImage"]?.ToString()?.Trim('\uFEFF');
                                currencySymbol = reader["CurrencySymbol"]?.ToString()?.Trim();
                            }
                        }
                    }
                }

                // Set currency symbol to label
                if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel && !string.IsNullOrEmpty(currencySymbol))
                {
                    currencyLabel.Text = currencySymbol;
                }

                if (string.IsNullOrWhiteSpace(svgText))
                {
                    SetCurrencyImageNull();
                    return;
                }

                Bitmap bitmap = null;
                try
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText)))
                    {
                        SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(stream);
                        bitmap = svgDoc.Draw();
                    }
                }
                catch
                {
                    bitmap = null;
                }

                if (bitmap == null)
                {
                    SetCurrencyImageNull();
                    return;
                }

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

                foreach (string name in pictureBoxNames)
                {
                    if (FindControl(name, true) is XRPictureBox pictureBox)
                    {
                        pictureBox.Image = bitmap;
                        pictureBox.Sizing = ImageSizeMode.Normal;
                    }
                }
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }
        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }

            if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel)
            {
                currencyLabel.Text = "";
            }
        }
    }
}
