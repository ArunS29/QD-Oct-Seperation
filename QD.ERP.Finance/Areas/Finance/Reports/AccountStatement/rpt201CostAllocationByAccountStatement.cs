using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Finance.Areas.Finance.Reports.AccountStatement
{
    public partial class rpt201CostAllocationByAccountStatement : DevExpress.XtraReports.UI.XtraReport
    {
        public rpt201CostAllocationByAccountStatement()
        {
            InitializeComponent();
        }
        public string VoucherNo { get; internal set; }

        public void LoadData(string voucherNo, string connectionString)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT * FROM [qry201RptVoucherWithCost] 
                                     WHERE VoucherNo = @VoucherNo"; // ✅ Fixed closing quote

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VoucherNo", voucherNo);

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
