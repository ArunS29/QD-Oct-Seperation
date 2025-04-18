namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
    partial class rpt20138BalanceSheetLiabilities
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
			DevExpress.DataAccess.Sql.QueryParameter queryParameter3 = new DevExpress.DataAccess.Sql.QueryParameter();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(rpt20138BalanceSheetLiabilities));
			DevExpress.XtraReports.UI.XRSummary xrSummary1 = new DevExpress.XtraReports.UI.XRSummary();
			DevExpress.XtraReports.UI.XRSummary xrSummary2 = new DevExpress.XtraReports.UI.XRSummary();
			this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
			this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
			this.Detail = new DevExpress.XtraReports.UI.DetailBand();
			this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
			this.GroupHeader1 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
			this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
			this.GroupHeader2 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
			this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
			this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
			this.calculatedField1 = new DevExpress.XtraReports.UI.CalculatedField();
			this.ReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
			this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// TopMargin
			// 
			this.TopMargin.HeightF = 25F;
			this.TopMargin.Name = "TopMargin";
			// 
			// BottomMargin
			// 
			this.BottomMargin.HeightF = 25F;
			this.BottomMargin.Name = "BottomMargin";
			// 
			// Detail
			// 
			this.Detail.HeightF = 0.3641625F;
			this.Detail.Name = "Detail";
			// 
			// sqlDataSource1
			// 
			this.sqlDataSource1.ConnectionName = "DBConnection";
			this.sqlDataSource1.Name = "sqlDataSource1";
			storedProcQuery1.Name = "sp20113BalanceSheet_ByGroup";
			queryParameter1.Name = "@EndDate";
			queryParameter1.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter1.Value = new DevExpress.DataAccess.Expression("\'4/12/2025 12:00:00 AM\'", typeof(System.DateTime));
			queryParameter2.Name = "@IsUseEffectiveDate";
			queryParameter2.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter2.Value = new DevExpress.DataAccess.Expression("False", typeof(bool));
			queryParameter3.Name = "@MasterGroupCategory";
			queryParameter3.Type = typeof(string);
			storedProcQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1,
            queryParameter2,
            queryParameter3});
			storedProcQuery1.StoredProcName = "sp20113BalanceSheet_ByGroup";
			this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
			this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
			// 
			// GroupHeader1
			// 
			this.GroupHeader1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLine1,
            this.xrLabel6,
            this.xrLabel5});
			this.GroupHeader1.HeightF = 42.91716F;
			this.GroupHeader1.Name = "GroupHeader1";
			// 
			// xrLine1
			// 
			this.xrLine1.LineStyle = DevExpress.Drawing.DXDashStyle.Dot;
			this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(22.91656F, 35.29167F);
			this.xrLine1.Name = "xrLine1";
			this.xrLine1.SizeF = new System.Drawing.SizeF(315.625F, 2.000015F);
			// 
			// xrLabel6
			// 
			this.xrLabel6.BackColor = System.Drawing.Color.Transparent;
			this.xrLabel6.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([AccountBalance])")});
			this.xrLabel6.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(221.0416F, 2.5F);
			this.xrLabel6.Multiline = true;
			this.xrLabel6.Name = "xrLabel6";
			this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel6.SizeF = new System.Drawing.SizeF(117.5F, 34.79168F);
			this.xrLabel6.StylePriority.UseBackColor = false;
			this.xrLabel6.StylePriority.UseBorders = false;
			this.xrLabel6.StylePriority.UseFont = false;
			xrSummary1.Running = DevExpress.XtraReports.UI.SummaryRunning.Group;
			this.xrLabel6.Summary = xrSummary1;
			this.xrLabel6.TextFormatString = "{0:0.00}";
			// 
			// xrLabel5
			// 
			this.xrLabel5.BackColor = System.Drawing.Color.Transparent;
			this.xrLabel5.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel5.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Upper([AccountGroup])")});
			this.xrLabel5.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel5.LocationFloat = new DevExpress.Utils.PointFloat(22.91656F, 0F);
			this.xrLabel5.Multiline = true;
			this.xrLabel5.Name = "xrLabel5";
			this.xrLabel5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel5.SizeF = new System.Drawing.SizeF(198.125F, 34.79168F);
			this.xrLabel5.StylePriority.UseBackColor = false;
			this.xrLabel5.StylePriority.UseBorders = false;
			this.xrLabel5.StylePriority.UseFont = false;
			// 
			// GroupHeader2
			// 
			this.GroupHeader2.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel4,
            this.xrLabel2});
			this.GroupHeader2.HeightF = 44.79167F;
			this.GroupHeader2.Level = 1;
			this.GroupHeader2.Name = "GroupHeader2";
			// 
			// xrLabel4
			// 
			this.xrLabel4.BackColor = System.Drawing.Color.DarkGray;
			this.xrLabel4.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
			this.xrLabel4.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([AccountBalance])")});
			this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(338.5417F, 0F);
			this.xrLabel4.Multiline = true;
			this.xrLabel4.Name = "xrLabel4";
			this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel4.SizeF = new System.Drawing.SizeF(145.8333F, 34.79168F);
			this.xrLabel4.StylePriority.UseBackColor = false;
			this.xrLabel4.StylePriority.UseBorders = false;
			this.xrLabel4.StylePriority.UseFont = false;
			xrSummary2.Running = DevExpress.XtraReports.UI.SummaryRunning.Group;
			this.xrLabel4.Summary = xrSummary2;
			this.xrLabel4.TextFormatString = "{0:0.00}";
			// 
			// xrLabel2
			// 
			this.xrLabel2.BackColor = System.Drawing.Color.DarkGray;
			this.xrLabel2.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
			this.xrLabel2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "Upper([MasterGroup])")});
			this.xrLabel2.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel2.Multiline = true;
			this.xrLabel2.Name = "xrLabel2";
			this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel2.SizeF = new System.Drawing.SizeF(338.5416F, 34.79168F);
			this.xrLabel2.StylePriority.UseBackColor = false;
			this.xrLabel2.StylePriority.UseBorders = false;
			this.xrLabel2.StylePriority.UseFont = false;
			// 
			// ReportHeader
			// 
			this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel7,
            this.xrLabel3});
			this.ReportHeader.HeightF = 37.50001F;
			this.ReportHeader.Name = "ReportHeader";
			// 
			// xrLabel3
			// 
			this.xrLabel3.BackColor = System.Drawing.Color.LightSkyBlue;
			this.xrLabel3.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
			this.xrLabel3.Font = new DevExpress.Drawing.DXFont("Cambria", 12F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(338.5417F, 0F);
			this.xrLabel3.Multiline = true;
			this.xrLabel3.Name = "xrLabel3";
			this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel3.SizeF = new System.Drawing.SizeF(145.8334F, 36.54167F);
			this.xrLabel3.StylePriority.UseBackColor = false;
			this.xrLabel3.StylePriority.UseBorders = false;
			this.xrLabel3.StylePriority.UseFont = false;
			this.xrLabel3.StylePriority.UseTextAlignment = false;
			this.xrLabel3.Text = "Amount";
			this.xrLabel3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// calculatedField1
			// 
			this.calculatedField1.DataMember = "sp20113BalanceSheet_ByGroup.Result1";
			this.calculatedField1.Name = "calculatedField1";
			// 
			// ReportFooter
			// 
			this.ReportFooter.HeightF = 0F;
			this.ReportFooter.Name = "ReportFooter";
			// 
			// xrLabel7
			// 
			this.xrLabel7.BackColor = System.Drawing.Color.LightSkyBlue;
			this.xrLabel7.Borders = ((DevExpress.XtraPrinting.BorderSide)((((DevExpress.XtraPrinting.BorderSide.Left | DevExpress.XtraPrinting.BorderSide.Top) 
            | DevExpress.XtraPrinting.BorderSide.Right) 
            | DevExpress.XtraPrinting.BorderSide.Bottom)));
			this.xrLabel7.Font = new DevExpress.Drawing.DXFont("Cambria", 12F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel7.Multiline = true;
			this.xrLabel7.Name = "xrLabel7";
			this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel7.SizeF = new System.Drawing.SizeF(338.5416F, 36.54167F);
			this.xrLabel7.StylePriority.UseBackColor = false;
			this.xrLabel7.StylePriority.UseBorders = false;
			this.xrLabel7.StylePriority.UseFont = false;
			this.xrLabel7.StylePriority.UseTextAlignment = false;
			this.xrLabel7.Text = "LIABILITIES";
			this.xrLabel7.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// rpt20138BalanceSheetLiabilities
			// 
			this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.GroupHeader1,
            this.GroupHeader2,
            this.ReportHeader,
            this.ReportFooter});
			this.CalculatedFields.AddRange(new DevExpress.XtraReports.UI.CalculatedField[] {
            this.calculatedField1});
			this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
			this.DataMember = "sp20113BalanceSheet_ByGroup";
			this.DataSource = this.sqlDataSource1;
			this.Font = new DevExpress.Drawing.DXFont("Cambria", 10F);
			this.Margins = new DevExpress.Drawing.DXMargins(25F, 334F, 25F, 25F);
			this.Version = "24.1";
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader1;
        private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader2;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel3;
        private DevExpress.XtraReports.UI.CalculatedField calculatedField1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel5;
        private DevExpress.XtraReports.UI.XRLabel xrLabel4;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.UI.ReportFooterBand ReportFooter;
        private DevExpress.XtraReports.UI.XRLine xrLine1;
		private DevExpress.XtraReports.UI.XRLabel xrLabel7;
	}
}
