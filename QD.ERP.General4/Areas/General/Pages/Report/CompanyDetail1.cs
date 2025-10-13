using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;


namespace QD.ERP.Web.Areas.General.Pages.Report
{
    public partial class CompanyDetail1 : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public CompanyDetail1()
        {
            InitializeComponent();
        }

        public CompanyDetail1(
            
            string CompanyId,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(CompanyId);
            LoadReportData(CompanyId);
        }

        private void SetReportParameters( string username)
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

   

           
        }

        private void LoadReportData(string CompanyId)
        {
            DataTable dt = GetReportData(CompanyId);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";

            // Get currencyId from the first row
        }


        private DataTable GetReportData(string CompanyId)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry90130CompanyMasterReport  WHERE CompanyID  = @CompanyID ";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@CompanyID ", CompanyId);

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

        public void CompanyDetail1_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            // Add your logic here if needed, or leave empty
        }
    }
}
