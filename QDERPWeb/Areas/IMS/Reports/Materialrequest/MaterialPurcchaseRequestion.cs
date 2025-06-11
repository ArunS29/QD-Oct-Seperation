using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.InventoryReports.MaterialPurchaseRequistion
{
    public partial class MaterialPurcchaseRequestion : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;

        public MaterialPurcchaseRequestion()
        {
            InitializeComponent();
        }

        public MaterialPurcchaseRequestion(
            string RequestNo,
            string tenantName,
            string companyName,
            string companyAddress,
            string companyNameAr,
            string companyAddressAr,
            bool isApproved,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _isApproved = isApproved;

            InitializeComponent();
            SetReportParameters(RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr);
            LoadReportData(RequestNo);
        }

        private void SetReportParameters(string RequestNo, string tenantName, string companyName, string companyAddress, string companyNameAr, string companyAddressAr)
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

            AddOrUpdateParameter("RequestNo", RequestNo, typeof(string));
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
        }

        private void LoadReportData(string RequestNo)
        {
            DataTable dt = GetReportData(RequestNo);

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

        private DataTable GetReportData(string RequestNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry606_05PurchaseRequestReport WHERE  MPRNo = @RequestNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@RequestNo", RequestNo);

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
    }
}
