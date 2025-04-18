namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
    partial class rpt20141IncomeStatement_IndirectExp
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
			DevExpress.XtraReports.UI.XRSummary xrSummary1 = new DevExpress.XtraReports.UI.XRSummary();
			DevExpress.XtraReports.UI.XRSummary xrSummary2 = new DevExpress.XtraReports.UI.XRSummary();
			DevExpress.DataAccess.Sql.StoredProcQuery storedProcQuery1 = new DevExpress.DataAccess.Sql.StoredProcQuery();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter1 = new DevExpress.DataAccess.Sql.QueryParameter();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter2 = new DevExpress.DataAccess.Sql.QueryParameter();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter3 = new DevExpress.DataAccess.Sql.QueryParameter();
			DevExpress.DataAccess.Sql.QueryParameter queryParameter4 = new DevExpress.DataAccess.Sql.QueryParameter();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(rpt20141IncomeStatement_IndirectExp));
			this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
			this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
			this.Detail = new DevExpress.XtraReports.UI.DetailBand();
			this.GroupHeader1 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
			this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
			this.GroupHeader2 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
			this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
			this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
			this.GroupFooter1 = new DevExpress.XtraReports.UI.GroupFooterBand();
			this.ReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			// 
			// TopMargin
			// 
			this.TopMargin.HeightF = 25F;
			this.TopMargin.Name = "TopMargin";
			// 
			// BottomMargin
			// 
			this.BottomMargin.HeightF = 131.4167F;
			this.BottomMargin.Name = "BottomMargin";
			// 
			// Detail
			// 
			this.Detail.HeightF = 0F;
			this.Detail.Name = "Detail";
			// 
			// GroupHeader1
			// 
			this.GroupHeader1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel1,
            this.xrLine1,
            this.xrLabel6,
            this.xrLabel5});
			this.GroupHeader1.HeightF = 43.5833F;
			this.GroupHeader1.Name = "GroupHeader1";
			// 
			// xrLabel1
			// 
			this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(22.91656F, 0F);
			this.xrLabel1.Multiline = true;
			this.xrLabel1.Name = "xrLabel1";
			this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel1.SizeF = new System.Drawing.SizeF(315.625F, 34.79171F);
			// 
			// xrLine1
			// 
			this.xrLine1.LineStyle = DevExpress.Drawing.DXDashStyle.Dot;
			this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(22.91657F, 34.79169F);
			this.xrLine1.Name = "xrLine1";
			this.xrLine1.SizeF = new System.Drawing.SizeF(461.4584F, 8.41666F);
			// 
			// xrLabel6
			// 
			this.xrLabel6.BackColor = System.Drawing.Color.Transparent;
			this.xrLabel6.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([Total])")});
			this.xrLabel6.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(186.8749F, 0F);
			this.xrLabel6.Multiline = true;
			this.xrLabel6.Name = "xrLabel6";
			this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel6.SizeF = new System.Drawing.SizeF(151.6666F, 34.79168F);
			this.xrLabel6.StylePriority.UseBackColor = false;
			this.xrLabel6.StylePriority.UseBorders = false;
			this.xrLabel6.StylePriority.UseFont = false;
			xrSummary1.Running = DevExpress.XtraReports.UI.SummaryRunning.Group;
			this.xrLabel6.Summary = xrSummary1;
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
			this.xrLabel5.SizeF = new System.Drawing.SizeF(163.9583F, 34.79168F);
			this.xrLabel5.StylePriority.UseBackColor = false;
			this.xrLabel5.StylePriority.UseBorders = false;
			this.xrLabel5.StylePriority.UseFont = false;
			// 
			// GroupHeader2
			// 
			this.GroupHeader2.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel4,
            this.xrLabel2});
			this.GroupHeader2.HeightF = 35.41667F;
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
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([Total])")});
			this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(338.5416F, 0F);
			this.xrLabel4.Multiline = true;
			this.xrLabel4.Name = "xrLabel4";
			this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel4.SizeF = new System.Drawing.SizeF(145.8334F, 34.79168F);
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
			this.ReportHeader.HeightF = 0F;
			this.ReportHeader.Name = "ReportHeader";
			// 
			// sqlDataSource1
			// 
			this.sqlDataSource1.ConnectionName = "DBConnection";
			this.sqlDataSource1.Name = "sqlDataSource1";
			storedProcQuery1.Name = "sp20101IncomeStatement_ByGroup";
			queryParameter1.Name = "@StartDate";
			queryParameter1.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter1.Value = new DevExpress.DataAccess.Expression("\'7/19/2024 12:00:00 AM\'", typeof(System.DateTime));
			queryParameter2.Name = "@EndDate";
			queryParameter2.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter2.Value = new DevExpress.DataAccess.Expression("\'4/12/2025 12:00:00 AM\'", typeof(System.DateTime));
			queryParameter3.Name = "@IsUseEffectiveDate";
			queryParameter3.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter3.Value = new DevExpress.DataAccess.Expression("False", typeof(bool));
			queryParameter4.Name = "@MasterGroupCategory";
			queryParameter4.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter4.Value = new DevExpress.DataAccess.Expression("", typeof(string));
			storedProcQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1,
            queryParameter2,
            queryParameter3,
            queryParameter4});
			storedProcQuery1.StoredProcName = "sp20101IncomeStatement_ByGroup";
			this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
			this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
			// 
			// GroupFooter1
			// 
			this.GroupFooter1.HeightF = 0F;
			this.GroupFooter1.Name = "GroupFooter1";
			// 
			// ReportFooter
			// 
			this.ReportFooter.HeightF = 0F;
			this.ReportFooter.Name = "ReportFooter";
			// 
			// rpt20141IncomeStatement_IndirectExp
			// 
			this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.GroupHeader1,
            this.GroupHeader2,
            this.ReportHeader,
            this.GroupFooter1,
            this.ReportFooter});
			this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
			this.DataMember = "sp20101IncomeStatement_ByGroup";
			this.DataSource = this.sqlDataSource1;
			this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
			this.Margins = new DevExpress.Drawing.DXMargins(25F, 340F, 25F, 131.4167F);
			this.Version = "24.1";
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader1;
        private DevExpress.XtraReports.UI.XRLine xrLine1;
        private DevExpress.XtraReports.UI.XRLabel xrLabel6;
        private DevExpress.XtraReports.UI.XRLabel xrLabel5;
        private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader2;
        private DevExpress.XtraReports.UI.XRLabel xrLabel4;
        private DevExpress.XtraReports.UI.XRLabel xrLabel2;
        private DevExpress.XtraReports.UI.ReportHeaderBand ReportHeader;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
        private DevExpress.XtraReports.UI.GroupFooterBand GroupFooter1;
        private DevExpress.XtraReports.UI.ReportFooterBand ReportFooter;
        private DevExpress.XtraReports.UI.XRLabel xrLabel1;
    }
}
