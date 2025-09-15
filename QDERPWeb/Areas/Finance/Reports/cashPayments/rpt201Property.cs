using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;

namespace QD.ERP.Web.Areas.Finance.Reports.cashPayments
{
    public partial class rpt201Property : DevExpress.XtraReports.UI.XtraReport
    {
        public rpt201Property()
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
                    string query = @"SELECT * FROM [qry20124SubLedgerForVouchers]
                                     WHERE VoucherNo = @VoucherNo AND DrCr = @DrCr";

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
