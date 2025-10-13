using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using QD.ERP.Finance.Areas.Finance.Reports.AccountStatement;
using QD.ERP.Shared.Service;
using DevExpress.Data.Svg;
using System;
using System.Drawing;
using System.Text;
using Svg;

namespace QD.ERP.Finance.Areas.Finance.Reports
{
    public partial class AccountDetails : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly Tenant _resolvedTenant;
        public AccountDetails(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out _resolvedTenant, out _))
                throw new Exception("Unable to resolve tenant context during report creation.");
            InitializeComponent();
            // ✅ Hook BeforePrint event handlers for subreports
            XrSubreport1.BeforePrint += XrSubreport1_BeforePrint;
            //XrSubreport2.BeforePrint += XrSubreport2_BeforePrint;
            //XrSubreport3.BeforePrint += XrSubreport3_BeforePrint;
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb, username);

            try
            {
                sqlDataSource1.Fill();

                LoadCurrencyImage(accountId, frmDate, toDate);

            }
            catch (Exception ex)
            {
                throw new Exception("Error loading report data: " + ex.Message, ex);
            }
        }

        public AccountDetails()
        {
            InitializeComponent();
        }

        private void XrSubreport1_BeforePrint(object sender, EventArgs e)
        {
            var subReportControl = (XRSubreport)sender;

            var voucherNoObj = GetCurrentColumnValue("VoucherEntryNo");
            if (voucherNoObj == null)
            {
                subReportControl.ReportSource = null;
                return;
            }

            string voucherNo = voucherNoObj.ToString().Trim();

            if (string.IsNullOrEmpty(voucherNo))
            {
                subReportControl.ReportSource = null;
                return;
            }

            var report = new rpt20140EmpAllocForAccountStatement();

            if (_resolvedTenant != null)
            {
                // Convert to int if DB expects int
                report.LoadData(voucherNo, _resolvedTenant.ConnectionString);
            }

            subReportControl.ReportSource = report;
        }



        //// ---------------- Subreport 2 ----------------
        //private void XrSubreport2_BeforePrint(object sender, EventArgs e)
        //{
        //    var subReportControl = (XRSubreport)sender;

        //    var voucherNo = GetCurrentColumnValue("VoucherEntryNo")?.ToString();

        //    var report = new rpt20124SubLedgerForAccountStatement
        //    {
        //        VoucherNo = voucherNo,
        //    };

        //    if (_resolvedTenant != null) // ✅ use the already resolved tenant
        //    {
        //        report.LoadData(voucherNo, _resolvedTenant.ConnectionString);
        //    }

        //    subReportControl.ReportSource = report;
        //}

        //// ---------------- Subreport 3 ----------------
        //private void XrSubreport3_BeforePrint(object sender, EventArgs e)
        //{
        //    var subReportControl = (XRSubreport)sender;

        //    var voucherNo = GetCurrentColumnValue("VoucherEntryNo")?.ToString();

        //    var report = new rpt201CostAllocationByAccountStatement
        //    {
        //        VoucherNo = voucherNo,
        //    };

        //    if (_resolvedTenant != null) // ✅ use the already resolved tenant
        //    {
        //        report.LoadData(voucherNo, _resolvedTenant.ConnectionString);
        //    }

        //    subReportControl.ReportSource = report;
        //}

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb, string username)
        {
            accountId ??= "";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;

            ConfigureSqlDataSource(accountId, frmDate, toDate);
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

        private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource1.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                // Use schema from tenant, or default to dbo
                string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string fullStoredProcName = $"{schemaName}.StProAccountLedger";

                var storedProcQuery = new StoredProcQuery
                {
                    Name = "StProAccountLedger",
                    StoredProcName = fullStoredProcName
                };

                storedProcQuery.Parameters.AddRange(new[]
                {
            new QueryParameter("@ParamAccountNo", typeof(string), accountId),
            new QueryParameter("@StartDate", typeof(DateTime), frmDate),
            new QueryParameter("@EndDate", typeof(DateTime), toDate)
        });

                sqlDataSource1.Queries.Add(storedProcQuery);
                sqlDataSource1.Name = "sqlDataSource1";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }
        private void LoadCurrencyImage(string accountId, DateTime frmDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                SetCurrencyImageNull();
                return;
            }

            try
            {
                if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string connectionString = tenant.ConnectionString;
                string svgText = null;

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new System.Data.SqlClient.SqlCommand($"{tenant.schemaname}.StProAccountLedger", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ParamAccountNo", accountId);
                        command.Parameters.AddWithValue("@StartDate", frmDate);
                        command.Parameters.AddWithValue("@EndDate", toDate);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("CurrencyImage")))
                            {
                                svgText = reader["CurrencyImage"]?.ToString()?.Trim().TrimStart('\uFEFF');
                            }
                        }
                    }
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

                if (FindControl("xrPictureBox2", true) is XRPictureBox pictureBoxDr)
                {
                    pictureBoxDr.Image = bitmap;
                    pictureBoxDr.Sizing = ImageSizeMode.Normal;
                }

                if (FindControl("xrPictureBox3", true) is XRPictureBox pictureBoxCr)
                {
                    pictureBoxCr.Image = bitmap;
                    pictureBoxCr.Sizing = ImageSizeMode.Normal;
                }

                if (FindControl("xrPictureBox4", true) is XRPictureBox pictureBox4)
                {
                    pictureBox4.Image = bitmap;
                    pictureBox4.Sizing = ImageSizeMode.Normal;
                }

                if (FindControl("xrPictureBox5", true) is XRPictureBox pictureBox5)
                {
                    pictureBox5.Image = bitmap;
                    pictureBox5.Sizing = ImageSizeMode.Normal;
                }

                if (FindControl("xrPictureBox6", true) is XRPictureBox pictureBox6)
                {
                    pictureBox6.Image = bitmap;
                    pictureBox6.Sizing = ImageSizeMode.Normal;
                }

                // NEW: Align bitmap next to currency fields dynamically
                AlignCurrencyWithAmount(bitmap);
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }

        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames =
            {
        "xrPictureBox2", "xrPictureBox3", "xrPictureBox4",
        "xrPictureBox5", "xrPictureBox6", "xrPictureBox7","xrPictureBox11","xrPictureBox12"
    };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }
        }

        // NEW: This method aligns the currency icon next to the matching labels
        private void AlignCurrencyWithAmount(Bitmap bitmap)
        {
            var pairs = new[]
            {
        new { Label = "xrLabel25", Picture = "xrPictureBox2" },
        new { Label = "xrLabel26", Picture = "xrPictureBox3" },
        new { Label = "xrLabel5", Picture = "xrPictureBox6" },
                new { Label = "xrLabel14", Picture = "xrPictureBox5" },
                new { Label = "xrLabel9", Picture = "xrPictureBox4" },
                new { Label = "xrLabel6", Picture = "xrPictureBox11" },
                new { Label = "xrLabel8", Picture = "xrPictureBox12" },


    };

            foreach (var p in pairs)
            {
                var label = FindControl(p.Label, true) as XRLabel;
                var pictureBox = FindControl(p.Picture, true) as XRPictureBox;

                if (label == null || pictureBox == null)
                    continue;

                pictureBox.Image = bitmap;
                pictureBox.Sizing = ImageSizeMode.StretchImage;

                label.BeforePrint += (s, e) =>
                {
                    var lbl = (XRLabel)s;

                    float iconWidth = 10f;
                    float iconHeight = 10f;

                    pictureBox.WidthF = iconWidth;
                    pictureBox.HeightF = iconHeight;

                    // Center the icon vertically with respect to the label
                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

                    // Convert DXFont to System.Drawing.Font manually
                    using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                    {
                        using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
                        {
                            var format = StringFormat.GenericTypographic;
                            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                            float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;

                            // Align image to left of text with 5 units padding
                            float rightEdge = lbl.LocationF.X + lbl.WidthF;
                            float posX = rightEdge - textWidth - iconWidth - 8f; // Adjusted spacing for visual gap

                            pictureBox.LocationF = new PointF(posX, posY);
                        }
                    }
                };
            }
        }
    }
}
