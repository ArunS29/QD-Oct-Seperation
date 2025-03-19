using System;
using System.Data;
using System.Data.SqlClient;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace QD.ERP.Web.Areas.Finance.Reports.test
{
    public partial class cashPayments : XtraReport
    {
        public cashPayments(string voucherNo)
        {
            InitializeComponent();
            SetReportParameters(voucherNo);
            LoadReportData(voucherNo);
        }

        private void SetReportParameters(string voucherNo)
        {
            Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
            {
                Name = "VoucherNo",
                Type = typeof(string),
                Value = voucherNo,
                Visible = false
            });
        }

        private void LoadReportData(string voucherNo)
        {
            DataTable dt = GetReportData(voucherNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                this.CreateNoDataLabel();
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";
            }
        }

        private DataTable GetReportData(string voucherNo)
        {
            DataTable dt = new DataTable();

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string connectionString = configuration.GetConnectionString("DbConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM [dbo].[qry201MainVoucherEntriesWithMaster] WHERE voucherno = @VoucherNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VoucherNo", voucherNo);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        conn.Open();
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching report data: {ex.Message}");
            }

            return dt;
        }

        private void CreateNoDataLabel()
        {
            XRLabel noDataLabel = new XRLabel
            {
                Text = "No records found.",
                BoundsF = new System.Drawing.RectangleF(0, 0, PageWidth - Margins.Left - Margins.Right, 50),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            };
            this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
        }
    }
}