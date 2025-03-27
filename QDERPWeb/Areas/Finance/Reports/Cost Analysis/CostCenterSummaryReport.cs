using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis
{
    public partial class CostCenterSummaryReport : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly string _connectionString;
        private string costAllocationMasterGroup;
        private DateTime frmDate;
        private DateTime toDate;
        private string tenantName;
        private string companyName;
        private string companyAddress;
        private Image logoImage;
        private string companyNameAr;
        private string companyAddressAr;

        public CostCenterSummaryReport(IConfiguration configuration,string costAllocationMasterGroup, DateTime frmDate, DateTime toDate,string tenantName, string company_Name, string company_address,Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            // Fetch connection string from appsettings.json
            _connectionString = configuration.GetConnectionString("DBConnection");
            SetReportParameters(costAllocationMasterGroup, frmDate, toDate, tenantName, company_Name,company_address, logoImage, Company_Name_Ar, company_address_arb);
            LoadReportData(costAllocationMasterGroup, frmDate, toDate);

        }

        

        // Parameterless constructor for design mode
        public CostCenterSummaryReport()
        {
            InitializeComponent();
            
        }

        private void SetReportParameters(string costAllocationMasterGroup, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address,Image logoImage, string Company_Name_Ar, string company_address_arb)
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

            // Ensure valid parameters
            costAllocationMasterGroup ??= "DefaultType";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            // Add or update report parameters
            AddOrUpdateParameter("CostAllocationMasterGroup", costAllocationMasterGroup, typeof(string));
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }
            if (this.FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.Text = company_Name;
            }
            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            {
                addressLabel.Text = company_address;
            }
            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.Text = Company_Name_Ar;
            }
            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            {
                addressArbLabel.Text = company_address_arb;
            }
            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }
        }

        private void LoadReportData(string costAllocationMasterGroup, DateTime frmDate, DateTime toDate)
        {
            SqlDataSource sqlDataSource = new SqlDataSource(_connectionString);

            CustomSqlQuery query = new CustomSqlQuery();
            query.Name = "CostCenterSummary";
            query.Sql = @"SELECT * FROM qry20151CostAnalysisReport 
                          WHERE CostAllocationMasterGroup = @CostAllocationMasterGroup 
                          AND TransactionDate BETWEEN @StartDate AND @EndDate";

            // Add parameters
            query.Parameters.Add(new QueryParameter("@CostAllocationMasterGroup", typeof(string), costAllocationMasterGroup));
            query.Parameters.Add(new QueryParameter("@StartDate", typeof(DateTime), frmDate));
            query.Parameters.Add(new QueryParameter("@EndDate", typeof(DateTime), toDate));

            sqlDataSource.Queries.Add(query);
            sqlDataSource.Fill();

            this.DataSource = sqlDataSource;
            this.DataMember = "CostCenterSummary";
        }
    }
}
