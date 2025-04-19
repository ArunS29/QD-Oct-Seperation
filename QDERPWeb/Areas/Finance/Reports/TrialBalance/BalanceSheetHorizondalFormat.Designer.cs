namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
    partial class BalanceSheetHorizondalFormat
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery1 = new DevExpress.DataAccess.Sql.StoredProcQuery();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter2 = new DevExpress.DataAccess.Sql.QueryParameter();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BalanceSheetHorizondalFormat));
            DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery2 = new DevExpress.DataAccess.Sql.StoredProcQuery();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter3 = new DevExpress.DataAccess.Sql.QueryParameter();
            DevExpress.DataAccess.Sql.QueryParameter queryParameter4 = new DevExpress.DataAccess.Sql.QueryParameter();
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.xrSubreport1 = new DevExpress.XtraReports.UI.XRSubreport();
            this.xrSubreport2 = new DevExpress.XtraReports.UI.XRSubreport();
            this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.sqlDataSource2 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            this.ReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
            this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
            this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
            this.xrPictureBox1 = new DevExpress.XtraReports.UI.XRPictureBox();
            this.xrLabel32 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
            this.xrLabel31 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel30 = new DevExpress.XtraReports.UI.XRLabel();
            this.xrLabel41 = new DevExpress.XtraReports.UI.XRLabel();
            this.PageFooter = new DevExpress.XtraReports.UI.PageFooterBand();
            this.xrLabel21 = new DevExpress.XtraReports.UI.XRLabel();
            this.pageInfo1 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.pageInfo2 = new DevExpress.XtraReports.UI.XRPageInfo();
            this.xrLabel28 = new DevExpress.XtraReports.UI.XRLabel();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 29.58334F;
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 25F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrSubreport1,
            this.xrSubreport2,
            this.xrLabel7,
            this.xrLabel6});
            this.Detail.HeightF = 98.79163F;
            this.Detail.Name = "Detail";
            // 
            // xrSubreport1
            // 
            this.xrSubreport1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 5.086263E-05F);
            this.xrSubreport1.Name = "xrSubreport1";
            this.xrSubreport1.ReportSource = new QD.ERP.Web.Areas.Finance.Reports.TrialBalance.rpt20138BalanceSheetAssets();
            this.xrSubreport1.SizeF = new System.Drawing.SizeF(716.25F, 88.79163F);
            // 
            // xrSubreport2
            // 
            this.xrSubreport2.LocationFloat = new DevExpress.Utils.PointFloat(728.7499F, 0F);
            this.xrSubreport2.Name = "xrSubreport2";
            this.xrSubreport2.ReportSource = new QD.ERP.Web.Areas.Finance.Reports.TrialBalance.rpt20138BalanceSheetLiabilities();
            this.xrSubreport2.SizeF = new System.Drawing.SizeF(727.2501F, 88.79167F);
            // 
            // xrLabel7
            // 
            this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(899.0833F, 0F);
            this.xrLabel7.Multiline = true;
            this.xrLabel7.Name = "xrLabel7";
            this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel7.SizeF = new System.Drawing.SizeF(144.9167F, 88.79166F);
            // 
            // xrLabel6
            // 
            this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrLabel6.Multiline = true;
            this.xrLabel6.Name = "xrLabel6";
            this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel6.SizeF = new System.Drawing.SizeF(514.5833F, 88.79166F);
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "DBConnection";
            this.sqlDataSource1.Name = "sqlDataSource1";
            storedProcQuery1.Name = "sp20113BalanceSheet";
            queryParameter1.Name = "@EndDate";
            queryParameter1.Type = typeof(global::System.DateTime);
            queryParameter1.ValueInfo = "1753-01-01";
            queryParameter2.Name = "@IsUseEffectiveDate";
            queryParameter2.Type = typeof(bool);
            queryParameter2.ValueInfo = "False";
            storedProcQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1,
            queryParameter2});
            storedProcQuery1.StoredProcName = "sp20113BalanceSheet";
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // sqlDataSource2
            // 
            this.sqlDataSource2.ConnectionName = "DBConnection";
            this.sqlDataSource2.Name = "sqlDataSource2";
            storedProcQuery2.Name = "sp20113BalanceSheet";
            queryParameter3.Name = "@EndDate";
            queryParameter3.Type = typeof(global::DevExpress.DataAccess.Expression);
            queryParameter3.Value = new DevExpress.DataAccess.Expression("\'4/10/2025 12:00:00 AM\'", typeof(System.DateTime));
            queryParameter4.Name = "@IsUseEffectiveDate";
            queryParameter4.Type = typeof(global::DevExpress.DataAccess.Expression);
            queryParameter4.Value = new DevExpress.DataAccess.Expression("False", typeof(bool));
            storedProcQuery2.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter3,
            queryParameter4});
            storedProcQuery2.StoredProcName = "sp20113BalanceSheet";
            this.sqlDataSource2.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery2});
            this.sqlDataSource2.ResultSchemaSerializable = resources.GetString("sqlDataSource2.ResultSchemaSerializable");
            // 
            // ReportFooter
            // 
            this.ReportFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel5,
            this.xrLabel4,
            this.xrLabel3,
            this.xrLabel2});
            this.ReportFooter.HeightF = 53.91674F;
            this.ReportFooter.Name = "ReportFooter";
            // 
            // xrLabel5
            // 
            this.xrLabel5.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.xrLabel5.LocationFloat = new DevExpress.Utils.PointFloat(1308.5F, 0F);
            this.xrLabel5.Multiline = true;
            this.xrLabel5.Name = "xrLabel5";
            this.xrLabel5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel5.SizeF = new System.Drawing.SizeF(147.5001F, 32.99999F);
            this.xrLabel5.StylePriority.UseBackColor = false;
            // 
            // xrLabel4
            // 
            this.xrLabel4.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(742.0833F, 0F);
            this.xrLabel4.Multiline = true;
            this.xrLabel4.Name = "xrLabel4";
            this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel4.SizeF = new System.Drawing.SizeF(566.4167F, 32.99998F);
            this.xrLabel4.StylePriority.UseBackColor = false;
            this.xrLabel4.StylePriority.UseFont = false;
            this.xrLabel4.StylePriority.UseTextAlignment = false;
            this.xrLabel4.Text = "Total :";
            this.xrLabel4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            // 
            // xrLabel3
            // 
            this.xrLabel3.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(514.5833F, 0F);
            this.xrLabel3.Multiline = true;
            this.xrLabel3.Name = "xrLabel3";
            this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel3.SizeF = new System.Drawing.SizeF(201.6667F, 32.99999F);
            this.xrLabel3.StylePriority.UseBackColor = false;
            // 
            // xrLabel2
            // 
            this.xrLabel2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.xrLabel2.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.xrLabel2.Multiline = true;
            this.xrLabel2.Name = "xrLabel2";
            this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel2.SizeF = new System.Drawing.SizeF(514.5833F, 32.99998F);
            this.xrLabel2.StylePriority.UseBackColor = false;
            this.xrLabel2.StylePriority.UseFont = false;
            this.xrLabel2.StylePriority.UseTextAlignment = false;
            this.xrLabel2.Text = "Total :";
            this.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
            // 
            // ReportHeader
            // 
            this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrPictureBox1,
            this.xrLabel32,
            this.xrLine1,
            this.xrLabel31,
            this.xrLabel30,
            this.xrLabel41});
            this.ReportHeader.HeightF = 154.5833F;
            this.ReportHeader.Name = "ReportHeader";
            // 
            // xrPictureBox1
            // 
            this.xrPictureBox1.BorderColor = System.Drawing.Color.Transparent;
            this.xrPictureBox1.LocationFloat = new DevExpress.Utils.PointFloat(48.68679F, 3.250138F);
            this.xrPictureBox1.Name = "xrPictureBox1";
            this.xrPictureBox1.SizeF = new System.Drawing.SizeF(126.6667F, 75.49998F);
            this.xrPictureBox1.Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage;
            this.xrPictureBox1.StylePriority.UseBorderColor = false;
            // 
            // xrLabel32
            // 
            this.xrLabel32.BorderColor = System.Drawing.Color.Transparent;
            this.xrLabel32.Font = new DevExpress.Drawing.DXFont("Arial", 11F);
            this.xrLabel32.ForeColor = System.Drawing.Color.DodgerBlue;
            this.xrLabel32.LocationFloat = new DevExpress.Utils.PointFloat(0F, 92.99998F);
            this.xrLabel32.Multiline = true;
            this.xrLabel32.Name = "xrLabel32";
            this.xrLabel32.SizeF = new System.Drawing.SizeF(1456F, 47.00002F);
            this.xrLabel32.StylePriority.UseBorderColor = false;
            this.xrLabel32.StylePriority.UseFont = false;
            this.xrLabel32.StylePriority.UseForeColor = false;
            this.xrLabel32.StylePriority.UseTextAlignment = false;
            this.xrLabel32.Text = "\r\nFor the period starting from [Parameters.StartDate!dd MMM yyyy] to [Parameters." +
    "EndDate!dd MMM yyyy]\r\n";
            this.xrLabel32.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrLine1
            // 
            this.xrLine1.LineWidth = 2F;
            this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 78.7501F);
            this.xrLine1.Name = "xrLine1";
            this.xrLine1.SizeF = new System.Drawing.SizeF(1456F, 8.416649F);
            // 
            // xrLabel31
            // 
            this.xrLabel31.BackColor = System.Drawing.Color.Transparent;
            this.xrLabel31.Font = new DevExpress.Drawing.DXFont("Cambria", 18F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel31.ForeColor = System.Drawing.Color.DeepSkyBlue;
            this.xrLabel31.LocationFloat = new DevExpress.Utils.PointFloat(10F, 53.83334F);
            this.xrLabel31.Multiline = true;
            this.xrLabel31.Name = "xrLabel31";
            this.xrLabel31.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
            this.xrLabel31.SizeF = new System.Drawing.SizeF(1446F, 33.33342F);
            this.xrLabel31.StylePriority.UseBackColor = false;
            this.xrLabel31.StylePriority.UseFont = false;
            this.xrLabel31.StylePriority.UseForeColor = false;
            this.xrLabel31.StylePriority.UseTextAlignment = false;
            this.xrLabel31.Text = "  B A L A N C E   S H E E T";
            this.xrLabel31.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
            // 
            // xrLabel30
            // 
            this.xrLabel30.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrLabel30.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.CompanyAddress]")});
            this.xrLabel30.Font = new DevExpress.Drawing.DXFont("Cambria", 14F);
            this.xrLabel30.LocationFloat = new DevExpress.Utils.PointFloat(10F, 28.00003F);
            this.xrLabel30.Name = "xrLabel30";
            this.xrLabel30.SizeF = new System.Drawing.SizeF(1446F, 25.83332F);
            this.xrLabel30.StylePriority.UseBorders = false;
            this.xrLabel30.StylePriority.UseFont = false;
            this.xrLabel30.StylePriority.UseTextAlignment = false;
            this.xrLabel30.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // xrLabel41
            // 
            this.xrLabel41.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.xrLabel41.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.CompanyName]\n\n\n")});
            this.xrLabel41.Font = new DevExpress.Drawing.DXFont("Cambria", 16F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel41.LocationFloat = new DevExpress.Utils.PointFloat(11.99997F, 3.250135F);
            this.xrLabel41.Name = "xrLabel41";
            this.xrLabel41.SizeF = new System.Drawing.SizeF(1444F, 28.00004F);
            this.xrLabel41.StylePriority.UseBorders = false;
            this.xrLabel41.StylePriority.UseFont = false;
            this.xrLabel41.StylePriority.UseTextAlignment = false;
            this.xrLabel41.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
            // 
            // PageFooter
            // 
            this.PageFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel21,
            this.pageInfo1,
            this.pageInfo2,
            this.xrLabel28});
            this.PageFooter.HeightF = 52.08333F;
            this.PageFooter.Name = "PageFooter";
            // 
            // xrLabel21
            // 
            this.xrLabel21.BorderColor = System.Drawing.Color.Transparent;
            this.xrLabel21.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel21.ForeColor = System.Drawing.Color.Black;
            this.xrLabel21.LocationFloat = new DevExpress.Utils.PointFloat(611.1696F, 0F);
            this.xrLabel21.Name = "xrLabel21";
            this.xrLabel21.SizeF = new System.Drawing.SizeF(294.1035F, 15.49992F);
            this.xrLabel21.StylePriority.UseBorderColor = false;
            this.xrLabel21.StylePriority.UseFont = false;
            this.xrLabel21.StylePriority.UseForeColor = false;
            this.xrLabel21.Text = "Reported By:[Parameters.TenantName]";
            // 
            // pageInfo1
            // 
            this.pageInfo1.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.pageInfo1.LocationFloat = new DevExpress.Utils.PointFloat(845.8336F, 0F);
            this.pageInfo1.Name = "pageInfo1";
            this.pageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime;
            this.pageInfo1.SizeF = new System.Drawing.SizeF(147.4999F, 14.66663F);
            this.pageInfo1.StylePriority.UseFont = false;
            // 
            // pageInfo2
            // 
            this.pageInfo2.LocationFloat = new DevExpress.Utils.PointFloat(438.9583F, 0F);
            this.pageInfo2.Name = "pageInfo2";
            this.pageInfo2.SizeF = new System.Drawing.SizeF(95.83331F, 23F);
            this.pageInfo2.StylePriority.UseTextAlignment = false;
            this.pageInfo2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.pageInfo2.TextFormatString = "Page {0} of {1}";
            // 
            // xrLabel28
            // 
            this.xrLabel28.BorderColor = System.Drawing.Color.Transparent;
            this.xrLabel28.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
            this.xrLabel28.ForeColor = System.Drawing.Color.DarkGray;
            this.xrLabel28.LocationFloat = new DevExpress.Utils.PointFloat(9.999998F, 2.333133F);
            this.xrLabel28.Name = "xrLabel28";
            this.xrLabel28.SizeF = new System.Drawing.SizeF(165.3535F, 20.66685F);
            this.xrLabel28.StylePriority.UseBorderColor = false;
            this.xrLabel28.StylePriority.UseFont = false;
            this.xrLabel28.StylePriority.UseForeColor = false;
            this.xrLabel28.Text = "Quickdice ERP Solutions";
            // 
            // BalanceSheetHorizondalFormat
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.ReportFooter,
            this.ReportHeader,
            this.PageFooter});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1,
            this.sqlDataSource2});
            this.DataMember = "sp20113BalanceSheet";
            this.DataSource = this.sqlDataSource1;
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new DevExpress.Drawing.DXMargins(24F, 10F, 29.58334F, 25F);
            this.PageWidth = 1500;
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.Custom;
            this.Version = "24.1";
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource2;
        private DevExpress.XtraReports.UI.ReportFooterBand ReportFooter;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRLabel xrLabel41;
        private DevExpress.XtraReports.UI.XRLabel xrLabel30;
        private DevExpress.XtraReports.UI.XRLine xrLine1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel31;
        private DevExpress.XtraReports.UI.XRLabel xrLabel32;
        private DevExpress.XtraReports.UI.PageFooterBand PageFooter;
        private DevExpress.XtraReports.UI.XRLabel xrLabel28;
        private DevExpress.XtraReports.UI.XRPageInfo pageInfo2;
        private DevExpress.XtraReports.UI.XRPageInfo pageInfo1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel3;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel5;
        private DevExpress.XtraReports.UI.XRLabel xrLabel4;
        private DevExpress.XtraReports.UI.XRLabel xrLabel7;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.UI.XRPictureBox xrPictureBox1;
        private DevExpress.XtraReports.UI.XRSubreport xrSubreport2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel21;
        private DevExpress.XtraReports.UI.XRSubreport xrSubreport1;
    }
}
