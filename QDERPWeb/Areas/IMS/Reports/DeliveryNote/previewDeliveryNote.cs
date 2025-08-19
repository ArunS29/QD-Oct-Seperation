using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.Reports.DeliveryNote
{
    public partial class previewDeliveryNote : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public previewDeliveryNote()
        {
            InitializeComponent();
        }

        public previewDeliveryNote(
            string deliveryNoteNo,
            string tenantName,
            string companyName,
            Image logoImage,
            Image sealImage,
            string companyAddress,
            string companyNameAr,
            string companyAddressAr,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(deliveryNoteNo, tenantName, companyName, logoImage, sealImage, companyAddress, companyNameAr, companyAddressAr, username);
            LoadReportData(deliveryNoteNo);
        }

        private void SetReportParameters(
            string deliveryNoteNo, string tenantName, string companyName, Image logoImage, Image sealImage,
            string companyAddress, string companyNameAr, string companyAddressAr, string username)
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

            AddOrUpdateParameter("DeliveryNoteNo", deliveryNoteNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            // Set control values (adjust control names accordingly in your .repx/.cs layout)
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

            if (FindControl("UserName", true) is XRLabel usernameLabel)
                usernameLabel.Text = username;

            if (FindControl("xrPictureBox4", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrPictureBox2", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        //private void LoadReportData(string deliveryNoteNo)
        //{
        //    DataTable dt = GetReportData(deliveryNoteNo);

        //    if (dt.Rows.Count == 0)
        //    {
        //        this.DataSource = null;
        //    }
        //    else
        //    {
        //        this.DataSource = dt;
        //        this.DataMember = ""; // or set to a named dataset if used
        //    }
        //}
        private void LoadReportData(string deliveryNoteNo)
        {
            DataTable dt = GetReportData(deliveryNoteNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";
           
            // Get currencyId from the first row
            if (dt.Columns.Contains("currencyId") && dt.Rows[0]["currencyId"] != DBNull.Value)
            {
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string svgXml = GetCurrencySvgXml(currencyId);

                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] { "xrPictureBox3" })
                    {
                        if (FindControl(pictureBoxName, true) is XRPictureBox pictureBox)
                        {
                            try
                            {
                                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgXml)))
                                {
                                    var svgDoc = Svg.SvgDocument.Open<Svg.SvgDocument>(stream);
                                    Bitmap bitmap = svgDoc.Draw();
                                    pictureBox.Image = bitmap;
                                    pictureBox.Sizing = ImageSizeMode.ZoomImage;
                                    pictureBox.SizeF = new SizeF(15f, 15f); // adjust as needed
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to render SVG for {pictureBoxName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
        private DataTable GetReportData(string deliveryNoteNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry603_05DeliveryNoteReport WHERE DeliveryNoteNo = @DeliveryNoteNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@DeliveryNoteNo", deliveryNoteNo);

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
        private string GetCurrencySvgXml(int currencyId)
        {
            string svgXml = null;

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                    throw new Exception("Unable to retrieve tenant context.");

                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CurrencyImage FROM Tbl20169CurrencyExchange WHERE CurrencyExchangeId = @currencyId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@currencyId", currencyId);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            svgXml = result.ToString(); // Raw SVG XML as string
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving currency SVG: {ex.Message}");
            }

            return svgXml;
        }
    }
}
