using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

namespace QD.ERP.Finance.Areas.Finance.Reports.cashPayments
{
    public partial class rpt201empReport : DevExpress.XtraReports.UI.XtraReport
    {
        public rpt201empReport()
        {
            InitializeComponent();
        }
        public void LoadData(string voucherNo, string drCr, string connectionString)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"SELECT * FROM  [qry20140EmpAllocationForVouchers]
                        WHERE VoucherNo = @VoucherNo AND EmpAllocDrCr = @DrCr";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VoucherNo", voucherNo);
                        cmd.Parameters.AddWithValue("@DrCr", drCr);


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
