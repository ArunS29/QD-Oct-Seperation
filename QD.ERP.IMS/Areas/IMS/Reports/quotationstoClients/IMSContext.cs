using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.IMS.Areas.IMS.Reports.quotationstoClients
{
    public partial class IMSContext : DevExpress.XtraReports.UI.XtraReport
    {
        public IMSContext()
        {
            InitializeComponent();
        }

        public void LoadTerms(string quoteNo, string connectionString)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ConditionsText FROM tbl601_03QuotationTerms WHERE QuoteNo = @QuoteNo";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@QuoteNo", quoteNo);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    conn.Open();
                    da.Fill(dt);
                }
            }
            this.DataSource = dt;
            this.DataMember = ""; // or dt.TableName
        }
    }
}
