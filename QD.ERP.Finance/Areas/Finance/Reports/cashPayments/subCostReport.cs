using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing; // For Image type if you use it

namespace QD.ERP.Finance.Areas.Finance.Reports.cashPayments
{
    public partial class subCostReport : DevExpress.XtraReports.UI.XtraReport
    {
        public subCostReport()
        {
            InitializeComponent();

        }



        // Load data dynamically for subreport with tenant connection string and voucherNo
        public void LoadData(string voucherNo, string accountHeadName, string connectionString)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT * FROM [qry201RptVoucherWithCost] 
                             WHERE VoucherNo = @VoucherNo AND AccountHeadName = @AccountHeadName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VoucherNo", voucherNo);
                        cmd.Parameters.AddWithValue("@AccountHeadName", accountHeadName);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        conn.Open();
                        da.Fill(dt);
                    }
                }

                this.DataSource = dt;
                this.DataMember = dt.TableName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading subreport data: {ex.Message}");
            }
        }

    }

}
