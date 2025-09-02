using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.VAT.Reports.Inventory_Reports
{
    public partial class MaterialRequestInventory : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;
        private string _requestNo;
        public MaterialRequestInventory()
        {
            InitializeComponent();

        }

        public MaterialRequestInventory(
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
            _requestNo = RequestNo;

            InitializeComponent();
            SetReportParameters(RequestNo, tenantName, companyName, companyAddress, companyNameAr, companyAddressAr);
            LoadReportData(RequestNo);

            // Attach the BeforePrint event ONCE
            this.BeforePrint += MaterialRequestInventory_BeforePrint;
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
        //
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
                        string query = "SELECT * FROM qry606_05PurchaseRequestReport  WHERE MPRNo = @RequestNo";

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
        private int GetTypeOfMPR(string requestNo)
        {
            int typeOfMPR = 0;

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT ISNULL(TypeOfMPR,0) FROM tbl606_01PurchaseRequestMaster WHERE MPRNo = @RequestNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@RequestNo", requestNo);

                            conn.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                                typeOfMPR = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching TypeOfMPR: {ex.Message}");
            }

            return typeOfMPR;
        }

        private void SetLabelsVisibility(int typeOfMPR)
        {
            // Find all labels
            XRLabel lbl8 = FindControl("xrLabel8", true) as XRLabel;
            XRLabel lbl9 = FindControl("xrLabel9", true) as XRLabel;
            XRLabel lbl5 = FindControl("xrLabel5", true) as XRLabel;
            XRLabel lbl6 = FindControl("xrLabel6", true) as XRLabel;
            XRLabel lbl10 = FindControl("xrLabel10", true) as XRLabel;
            XRLabel lbl55 = FindControl("xrLabel55", true) as XRLabel;
            XRLabel lbl25 = FindControl("xrLabel25", true) as XRLabel;
            XRLabel lbl11 = FindControl("xrLabel11", true) as XRLabel;
            XRLabel lbl12 = FindControl("xrLabel12", true) as XRLabel;
            XRLabel lbl13 = FindControl("xrLabel13", true) as XRLabel;
            XRLabel lbl14 = FindControl("xrLabel14", true) as XRLabel;
            XRLabel lbl4 = FindControl("xrLabel4", true) as XRLabel;
            XRLabel lbl69 = FindControl("xrLabel69", true) as XRLabel;
            XRLabel lbl70 = FindControl("xrLabel70", true) as XRLabel;
            XRLabel lbl71 = FindControl("xrLabel71", true) as XRLabel;
            XRLabel lbl72 = FindControl("xrLabel72", true) as XRLabel;

            // Hide all labels first
            foreach (var lbl in new[] { lbl8, lbl9, lbl5, lbl6, lbl10, lbl55, lbl25, lbl11, lbl12, lbl13, lbl14, lbl4, lbl69, lbl70, lbl71, lbl72 })
            {
                if (lbl != null) lbl.Visible = false;
            }

            // Show labels based on TypeOfMPR
            switch (typeOfMPR)
            {
                case 1:
                    ShowLabels(lbl6, lbl10, lbl5, lbl11, lbl8, lbl13, lbl14, lbl55, lbl9,  lbl25);
                    break;
                case 2:
                    ShowLabels(lbl4, lbl10, lbl71, lbl69, lbl8, lbl13, lbl14, lbl55, lbl9, lbl25);
                    break;
                case 3:
                    ShowLabels(lbl70, lbl10, lbl71, lbl11, lbl8, lbl13, lbl14, lbl55, lbl9, lbl25);
                    break;
                default:
                    // Optionally show a message or log if TypeOfMPR is not 1, 2, or 3
                    System.Diagnostics.Debug.WriteLine($"TypeOfMPR value '{typeOfMPR}' does not match any label set.");
                    break;
            }

            foreach (var name in new[] { "xrLabel10", "xrLabel69", "xrLabel72", "xrLabel11", "xrLabel12", "xrLabel5", "xrLabel4", "xrLabel13", "xrLabel14", "xrLabel8", "xrLabel9", "xrLabel55", "xrLabel25", "xrLabel6", "xrLabel70", "xrLabel71" })
            {
                var lbl = FindControl(name, true) as XRLabel;
                System.Diagnostics.Debug.WriteLine($"{name}: {(lbl != null ? "Found" : "Not Found")}, Visible: {lbl?.Visible}");
            }
        }

        private void ShowLabels(params XRLabel[] labels)
        {
            foreach (var lbl in labels)
            {
                if (lbl != null)
                {
                    lbl.Visible = true;
                    System.Diagnostics.Debug.WriteLine($"{lbl.Name} set to Visible");
                }
            }
        }

        // Ensure the handler is 'protected' or 'public' and matches the expected signature for BeforePrintEventHandler
        private void MaterialRequestInventory_BeforePrint(object sender, EventArgs e)
        {
            int typeOfMPR = GetTypeOfMPR(_requestNo);
            SetLabelsVisibility(typeOfMPR);
        }
    }
}
