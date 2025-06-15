using System.Net.Http;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using DevExpress.XtraReports.UI;
using System.Drawing;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting.Drawing;

namespace QD.ERP.Web.Areas.VAT.Reports.B2B_INVOICE
{
    public partial class TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;  // This flag will come from the client-side

        public TAXINVOICEWTDOCUMENTALLEVELDISCOUNTSS(
            string invoiceNo,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            Image sealImage,
            string companyNameAr,
            string companyAddressAr,
            bool isApproved,  // Accepting approval status here
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _isApproved = isApproved;  // Store the approval status

            InitializeComponent();
            SetReportParameters(invoiceNo, tenantName, companyName, companyAddress, logoImage,  sealImage, companyNameAr, companyAddressAr);
            LoadReportData(invoiceNo);  // Load the data synchronously
        }

        private void SetReportParameters(string invoiceNo, string tenantName, string companyName, string companyAddress, Image logoImage, Image sealImage, string companyNameAr, string companyAddressAr)
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

            AddOrUpdateParameter("InvoiceNo", invoiceNo, typeof(string));
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

            if (FindControl("xrPictureBox2", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
            if (FindControl("xrPictureBox5", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        private void LoadReportData(string invoiceNo)
        {
            DataTable dt = GetReportData(invoiceNo);  // Load data synchronously

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
               
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";
                SetWatermark();
            }
        }

        // GetReportData is implemented below:
        private DataTable GetReportData(string invoiceNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry201_602VATInvoiceReport WHERE InvoiceNo = @InvoiceNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);

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

                // Set font
                this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);

                this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230); 

                this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;

                this.Watermark.ShowBehind = true;

                this.Watermark.ImageTiling = false;
                this.Watermark.ImageViewMode = ImageViewMode.Stretch;
            }
        }




       
    }
}
