using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting.Drawing;
using QD.ERP.Web.Service;
using System.ComponentModel;

namespace QD.ERP.Web.Areas.VAT.Reports.VATCreditNote
{
    public partial class CreditForeignCurrency : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;

        public CreditForeignCurrency(
            string creditNoteNo,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            bool isApproved,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _isApproved = isApproved;

            InitializeComponent();
            SetReportParameters(creditNoteNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
            LoadReportData(creditNoteNo);
        }

        private void SetReportParameters(string creditNoteNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                    {
                        Name = name,
                        Type = type,
                        Value = value,
                        Visible = visible
                    });
                }
                else
                {
                    Parameters[name].Value = value;
                    Parameters[name].Visible = visible;
                }
            }

            AddOrUpdateParameter("CreditNoteNo", creditNoteNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
        }

        private void LoadReportData(string creditNoteNo)
        {
            DataTable dt = GetReportData(creditNoteNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                CreateNoDataLabel();
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";
                SetWatermark();
            }
        }

        private DataTable GetReportData(string creditNoteNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry201_802VATCreditNoteReport WHERE CreditNoteNo = @CreditNoteNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@CreditNoteNo", creditNoteNo);

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            conn.Open();
                            da.Fill(dt);
                        }
                    }
                }
                else
                {
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching report data: {ex.Message}");
            }

            return dt;
        }

        private void SetWatermark()
        {
            if (!_isApproved)
            {
                this.Watermark.Text = "DRAFT COPY";
                this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);
                this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230);
                this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
                this.Watermark.ShowBehind = true;
                this.Watermark.ImageTiling = false;
                this.Watermark.ImageViewMode = ImageViewMode.Stretch;
            }
        }

        private void CreateNoDataLabel()
        {
            XRLabel noDataLabel = new XRLabel
            {
                Text = "No records found.",
                BoundsF = new RectangleF(0, 0, PageWidth - Margins.Left - Margins.Right, 50),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            };
            this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
        }

        private void CreditNotes_BeforePrint(object sender, CancelEventArgs e)
        {
            // Optional: Add logic before printing
        }
    }
}
