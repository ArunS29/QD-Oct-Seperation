using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.VAT.Reports.VATReturns
{
	public partial class VATReturnsform : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        public VATReturnsform()
		{
			InitializeComponent();
		}
       
    }
}
