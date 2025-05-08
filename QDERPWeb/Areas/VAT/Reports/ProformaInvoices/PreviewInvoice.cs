using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.VAT.PERFORMINVOICES
{
    public partial class PreviewInvoice : DevExpress.XtraReports.UI.XtraReport
    {
        public PreviewInvoice()
        {
            InitializeComponent();
        }

        private void xrLabel96_BeforePrint(object sender, CancelEventArgs e)
        {

        }
    }
}
