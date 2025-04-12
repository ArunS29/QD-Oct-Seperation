namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
	partial class incomestatement
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(incomestatement));
			DevExpress.XtraReports.UI.XRSummary xrSummary1 = new DevExpress.XtraReports.UI.XRSummary();
			this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
			this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
			this.Detail = new DevExpress.XtraReports.UI.DetailBand();
			this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
			this.GroupHeader1 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.GroupHeader2 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.ReportHeader = new DevExpress.XtraReports.UI.ReportHeaderBand();
			this.xrLabel1 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel2 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel3 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel4 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel5 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel6 = new DevExpress.XtraReports.UI.XRLabel();
			this.GroupHeader3 = new DevExpress.XtraReports.UI.GroupHeaderBand();
			this.xrLabel7 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel8 = new DevExpress.XtraReports.UI.XRLabel();
			this.GroupFooter1 = new DevExpress.XtraReports.UI.GroupFooterBand();
			this.ReportFooter = new DevExpress.XtraReports.UI.ReportFooterBand();
			this.xrLabel9 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel10 = new DevExpress.XtraReports.UI.XRLabel();
			this.GroupFooter2 = new DevExpress.XtraReports.UI.GroupFooterBand();
			this.xrLabel11 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel12 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel13 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel14 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel40 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel20 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel23 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel24 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLabel15 = new DevExpress.XtraReports.UI.XRLabel();
			this.xrLine1 = new DevExpress.XtraReports.UI.XRLine();
			this.PageFooter = new DevExpress.XtraReports.UI.PageFooterBand();
			this.xrLabel25 = new DevExpress.XtraReports.UI.XRLabel();
			this.pageInfo2 = new DevExpress.XtraReports.UI.XRPageInfo();
			this.xrLabel21 = new DevExpress.XtraReports.UI.XRLabel();
			this.pageInfo1 = new DevExpress.XtraReports.UI.XRPageInfo();
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
			this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel14,
            this.xrLabel13,
            this.xrLabel6});
			this.Detail.HeightF = 23F;
			this.Detail.Name = "Detail";
			// 
			// sqlDataSource1
			// 
			this.sqlDataSource1.ConnectionName = "DBConnection";
			this.sqlDataSource1.Name = "sqlDataSource1";
			storedProcQuery1.Name = "sp20101IncomeStatement";
			queryParameter1.Name = "@StartDate";
			queryParameter1.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter1.Value = new DevExpress.DataAccess.Expression("\'12/9/2024 12:00:00 AM\'", typeof(System.DateTime));
			queryParameter2.Name = "@EndDate";
			queryParameter2.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter2.Value = new DevExpress.DataAccess.Expression("\'4/11/2025 12:00:00 AM\'", typeof(System.DateTime));
			queryParameter3.Name = "@IsUseEffectiveDate";
			queryParameter3.Type = typeof(global::DevExpress.DataAccess.Expression);
			queryParameter3.Value = new DevExpress.DataAccess.Expression("False", typeof(bool));
			storedProcQuery1.Parameters.AddRange(new DevExpress.DataAccess.Sql.QueryParameter[] {
            queryParameter1,
            queryParameter2,
            queryParameter3});
			storedProcQuery1.StoredProcName = "sp20101IncomeStatement";
			this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            storedProcQuery1});
			this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
			// 
			// GroupHeader1
			// 
			this.GroupHeader1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel8});
			this.GroupHeader1.HeightF = 36.45833F;
			this.GroupHeader1.Name = "GroupHeader1";
			// 
			// GroupHeader2
			// 
			this.GroupHeader2.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel7,
            this.xrLabel5});
			this.GroupHeader2.HeightF = 31.25F;
			this.GroupHeader2.Level = 1;
			this.GroupHeader2.Name = "GroupHeader2";
			// 
			// ReportHeader
			// 
			this.ReportHeader.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLine1,
            this.xrLabel15,
            this.xrLabel24,
            this.xrLabel23,
            this.xrLabel20,
            this.xrLabel40});
			this.ReportHeader.HeightF = 186.4583F;
			this.ReportHeader.Name = "ReportHeader";
			// 
			// xrLabel1
			// 
			this.xrLabel1.BackColor = System.Drawing.SystemColors.MenuHighlight;
			this.xrLabel1.Font = new DevExpress.Drawing.DXFont("Arial", 10F, ((DevExpress.Drawing.DXFontStyle)((DevExpress.Drawing.DXFontStyle.Bold | DevExpress.Drawing.DXFontStyle.Italic))));
			this.xrLabel1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel1.Multiline = true;
			this.xrLabel1.Name = "xrLabel1";
			this.xrLabel1.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel1.SizeF = new System.Drawing.SizeF(125F, 33.4167F);
			this.xrLabel1.StylePriority.UseBackColor = false;
			this.xrLabel1.StylePriority.UseFont = false;
			this.xrLabel1.StylePriority.UseTextAlignment = false;
			this.xrLabel1.Text = "Code";
			this.xrLabel1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			this.xrLabel1.TextFitMode = DevExpress.XtraReports.UI.TextFitMode.GrowOnly;
			// 
			// xrLabel2
			// 
			this.xrLabel2.BackColor = System.Drawing.SystemColors.MenuHighlight;
			this.xrLabel2.Font = new DevExpress.Drawing.DXFont("Arial", 10F, ((DevExpress.Drawing.DXFontStyle)((DevExpress.Drawing.DXFontStyle.Bold | DevExpress.Drawing.DXFontStyle.Italic))));
			this.xrLabel2.LocationFloat = new DevExpress.Utils.PointFloat(125F, 0F);
			this.xrLabel2.Multiline = true;
			this.xrLabel2.Name = "xrLabel2";
			this.xrLabel2.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel2.SizeF = new System.Drawing.SizeF(329.1667F, 33.4167F);
			this.xrLabel2.StylePriority.UseBackColor = false;
			this.xrLabel2.StylePriority.UseFont = false;
			this.xrLabel2.StylePriority.UseTextAlignment = false;
			this.xrLabel2.Text = "Account";
			this.xrLabel2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			this.xrLabel2.TextFitMode = DevExpress.XtraReports.UI.TextFitMode.GrowOnly;
			// 
			// xrLabel3
			// 
			this.xrLabel3.BackColor = System.Drawing.SystemColors.MenuHighlight;
			this.xrLabel3.Font = new DevExpress.Drawing.DXFont("Arial", 10F, ((DevExpress.Drawing.DXFontStyle)((DevExpress.Drawing.DXFontStyle.Bold | DevExpress.Drawing.DXFontStyle.Italic))));
			this.xrLabel3.LocationFloat = new DevExpress.Utils.PointFloat(454.1667F, 0F);
			this.xrLabel3.Multiline = true;
			this.xrLabel3.Name = "xrLabel3";
			this.xrLabel3.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel3.SizeF = new System.Drawing.SizeF(169.7917F, 33.4167F);
			this.xrLabel3.StylePriority.UseBackColor = false;
			this.xrLabel3.StylePriority.UseFont = false;
			this.xrLabel3.StylePriority.UseTextAlignment = false;
			this.xrLabel3.Text = "Amount";
			this.xrLabel3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			this.xrLabel3.TextFitMode = DevExpress.XtraReports.UI.TextFitMode.GrowOnly;
			// 
			// xrLabel4
			// 
			this.xrLabel4.BackColor = System.Drawing.SystemColors.MenuHighlight;
			this.xrLabel4.Font = new DevExpress.Drawing.DXFont("Arial", 10F, ((DevExpress.Drawing.DXFontStyle)((DevExpress.Drawing.DXFontStyle.Bold | DevExpress.Drawing.DXFontStyle.Italic))));
			this.xrLabel4.LocationFloat = new DevExpress.Utils.PointFloat(623.9583F, 0F);
			this.xrLabel4.Multiline = true;
			this.xrLabel4.Name = "xrLabel4";
			this.xrLabel4.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel4.SizeF = new System.Drawing.SizeF(176.0417F, 33.4167F);
			this.xrLabel4.StylePriority.UseBackColor = false;
			this.xrLabel4.StylePriority.UseFont = false;
			this.xrLabel4.StylePriority.UseTextAlignment = false;
			this.xrLabel4.Text = "Amount";
			this.xrLabel4.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			this.xrLabel4.TextFitMode = DevExpress.XtraReports.UI.TextFitMode.GrowOnly;
			// 
			// xrLabel5
			// 
			this.xrLabel5.BackColor = System.Drawing.Color.Bisque;
			this.xrLabel5.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel5.Multiline = true;
			this.xrLabel5.Name = "xrLabel5";
			this.xrLabel5.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel5.SizeF = new System.Drawing.SizeF(800F, 29.25F);
			this.xrLabel5.StylePriority.UseBackColor = false;
			// 
			// xrLabel6
			// 
			this.xrLabel6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AccountHead]")});
			this.xrLabel6.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel6.Multiline = true;
			this.xrLabel6.Name = "xrLabel6";
			this.xrLabel6.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
			this.xrLabel6.SizeF = new System.Drawing.SizeF(143.75F, 23F);
			this.xrLabel6.Text = "xrLabel6";
			// 
			// GroupHeader3
			// 
			this.GroupHeader3.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel4,
            this.xrLabel3,
            this.xrLabel2,
            this.xrLabel1});
			this.GroupHeader3.HeightF = 36.45833F;
			this.GroupHeader3.Level = 2;
			this.GroupHeader3.Name = "GroupHeader3";
			// 
			// xrLabel7
			// 
			this.xrLabel7.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[MasterGroup]")});
			this.xrLabel7.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel7.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel7.Multiline = true;
			this.xrLabel7.Name = "xrLabel7";
			this.xrLabel7.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
			this.xrLabel7.SizeF = new System.Drawing.SizeF(317.7083F, 29.24997F);
			this.xrLabel7.StylePriority.UseFont = false;
			// 
			// xrLabel8
			// 
			this.xrLabel8.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AccountGroup]")});
			this.xrLabel8.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel8.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel8.Multiline = true;
			this.xrLabel8.Name = "xrLabel8";
			this.xrLabel8.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel8.SizeF = new System.Drawing.SizeF(317.7083F, 29.24997F);
			this.xrLabel8.StylePriority.UseFont = false;
			// 
			// GroupFooter1
			// 
			this.GroupFooter1.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel12,
            this.xrLabel11});
			this.GroupFooter1.HeightF = 32.29167F;
			this.GroupFooter1.Name = "GroupFooter1";
			// 
			// ReportFooter
			// 
			this.ReportFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel10});
			this.ReportFooter.HeightF = 43.66671F;
			this.ReportFooter.Name = "ReportFooter";
			// 
			// xrLabel9
			// 
			this.xrLabel9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.xrLabel9.Font = new DevExpress.Drawing.DXFont("Cambria", 10F);
			this.xrLabel9.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel9.Multiline = true;
			this.xrLabel9.Name = "xrLabel9";
			this.xrLabel9.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel9.SizeF = new System.Drawing.SizeF(799.9999F, 29.25F);
			this.xrLabel9.StylePriority.UseBackColor = false;
			this.xrLabel9.StylePriority.UseFont = false;
			this.xrLabel9.StylePriority.UseTextAlignment = false;
			this.xrLabel9.Text = "Total of   [MasterGroup] :";
			this.xrLabel9.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
			// 
			// xrLabel10
			// 
			this.xrLabel10.BackColor = System.Drawing.Color.Bisque;
			this.xrLabel10.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Total]")});
			this.xrLabel10.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel10.Multiline = true;
			this.xrLabel10.Name = "xrLabel10";
			this.xrLabel10.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel10.SizeF = new System.Drawing.SizeF(800F, 29.25F);
			this.xrLabel10.StylePriority.UseBackColor = false;
			this.xrLabel10.StylePriority.UseTextAlignment = false;
			this.xrLabel10.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
			// 
			// GroupFooter2
			// 
			this.GroupFooter2.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.xrLabel9});
			this.GroupFooter2.HeightF = 31.25F;
			this.GroupFooter2.Level = 1;
			this.GroupFooter2.Name = "GroupFooter2";
			// 
			// xrLabel11
			// 
			this.xrLabel11.BackColor = System.Drawing.Color.DarkGray;
			this.xrLabel11.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "sumSum([Total])")});
			this.xrLabel11.LocationFloat = new DevExpress.Utils.PointFloat(645.8333F, 0F);
			this.xrLabel11.Multiline = true;
			this.xrLabel11.Name = "xrLabel11";
			this.xrLabel11.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel11.SizeF = new System.Drawing.SizeF(154.1666F, 29.25F);
			this.xrLabel11.StylePriority.UseBackColor = false;
			this.xrLabel11.StylePriority.UseTextAlignment = false;
			xrSummary1.Running = DevExpress.XtraReports.UI.SummaryRunning.Group;
			this.xrLabel11.Summary = xrSummary1;
			this.xrLabel11.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
			this.xrLabel11.TextFormatString = "{0:0.00}";
			// 
			// xrLabel12
			// 
			this.xrLabel12.Font = new DevExpress.Drawing.DXFont("Cambria", 10F);
			this.xrLabel12.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel12.Multiline = true;
			this.xrLabel12.Name = "xrLabel12";
			this.xrLabel12.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 96F);
			this.xrLabel12.SizeF = new System.Drawing.SizeF(646.8749F, 29.24999F);
			this.xrLabel12.StylePriority.UseFont = false;
			this.xrLabel12.StylePriority.UseTextAlignment = false;
			this.xrLabel12.Text = "Total of [AccountGroup] :";
			this.xrLabel12.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
			// 
			// xrLabel13
			// 
			this.xrLabel13.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AccountHeadName]")});
			this.xrLabel13.LocationFloat = new DevExpress.Utils.PointFloat(143.75F, 0F);
			this.xrLabel13.Name = "xrLabel13";
			this.xrLabel13.SizeF = new System.Drawing.SizeF(280.2084F, 22.99999F);
			this.xrLabel13.Text = "[AccountHead]";
			// 
			// xrLabel14
			// 
			this.xrLabel14.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "-([Total])")});
			this.xrLabel14.LocationFloat = new DevExpress.Utils.PointFloat(423.9584F, 0F);
			this.xrLabel14.Name = "xrLabel14";
			this.xrLabel14.SizeF = new System.Drawing.SizeF(200F, 22.99999F);
			this.xrLabel14.StylePriority.UseTextAlignment = false;
			this.xrLabel14.Text = "[AccountHead]";
			this.xrLabel14.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel40
			// 
			this.xrLabel40.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel40.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.CompanyName]\n\n\n")});
			this.xrLabel40.Font = new DevExpress.Drawing.DXFont("Cambria", 16F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel40.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
			this.xrLabel40.Name = "xrLabel40";
			this.xrLabel40.SizeF = new System.Drawing.SizeF(799.9999F, 28.00004F);
			this.xrLabel40.StylePriority.UseBorders = false;
			this.xrLabel40.StylePriority.UseFont = false;
			this.xrLabel40.StylePriority.UseTextAlignment = false;
			this.xrLabel40.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
			// 
			// xrLabel20
			// 
			this.xrLabel20.Borders = DevExpress.XtraPrinting.BorderSide.None;
			this.xrLabel20.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[Parameters.CompanyAddress]")});
			this.xrLabel20.Font = new DevExpress.Drawing.DXFont("Cambria", 14F);
			this.xrLabel20.LocationFloat = new DevExpress.Utils.PointFloat(0F, 28.00004F);
			this.xrLabel20.Name = "xrLabel20";
			this.xrLabel20.SizeF = new System.Drawing.SizeF(799.9999F, 25.83332F);
			this.xrLabel20.StylePriority.UseBorders = false;
			this.xrLabel20.StylePriority.UseFont = false;
			this.xrLabel20.StylePriority.UseTextAlignment = false;
			this.xrLabel20.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
			// 
			// xrLabel23
			// 
			this.xrLabel23.BackColor = System.Drawing.Color.Transparent;
			this.xrLabel23.Font = new DevExpress.Drawing.DXFont("Cambria", 18F);
			this.xrLabel23.ForeColor = System.Drawing.Color.DeepSkyBlue;
			this.xrLabel23.LocationFloat = new DevExpress.Utils.PointFloat(0F, 48.49997F);
			this.xrLabel23.Multiline = true;
			this.xrLabel23.Name = "xrLabel23";
			this.xrLabel23.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel23.SizeF = new System.Drawing.SizeF(799.9999F, 33.33342F);
			this.xrLabel23.StylePriority.UseBackColor = false;
			this.xrLabel23.StylePriority.UseFont = false;
			this.xrLabel23.StylePriority.UseForeColor = false;
			this.xrLabel23.StylePriority.UseTextAlignment = false;
			this.xrLabel23.Text = "INCOME  STATEMENTS";
			this.xrLabel23.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter;
			// 
			// xrLabel24
			// 
			this.xrLabel24.BorderColor = System.Drawing.Color.Transparent;
			this.xrLabel24.Font = new DevExpress.Drawing.DXFont("Arial", 11F);
			this.xrLabel24.ForeColor = System.Drawing.Color.DodgerBlue;
			this.xrLabel24.LocationFloat = new DevExpress.Utils.PointFloat(0F, 92.99997F);
			this.xrLabel24.Multiline = true;
			this.xrLabel24.Name = "xrLabel24";
			this.xrLabel24.SizeF = new System.Drawing.SizeF(799.9999F, 47.00003F);
			this.xrLabel24.StylePriority.UseBorderColor = false;
			this.xrLabel24.StylePriority.UseFont = false;
			this.xrLabel24.StylePriority.UseForeColor = false;
			this.xrLabel24.StylePriority.UseTextAlignment = false;
			this.xrLabel24.Text = "\r\nFor the period starting from [Parameters.StartDate!dd MMM yyyy] to [Parameters." +
    "EndDate!dd MMM yyyy]\r\n";
			this.xrLabel24.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
			// 
			// xrLabel15
			// 
			this.xrLabel15.Font = new DevExpress.Drawing.DXFont("Cambria", 10F);
			this.xrLabel15.LocationFloat = new DevExpress.Utils.PointFloat(0F, 140F);
			this.xrLabel15.Multiline = true;
			this.xrLabel15.Name = "xrLabel15";
			this.xrLabel15.Padding = new DevExpress.XtraPrinting.PaddingInfo(2, 2, 0, 0, 100F);
			this.xrLabel15.SizeF = new System.Drawing.SizeF(799.9999F, 23F);
			this.xrLabel15.StylePriority.UseFont = false;
			this.xrLabel15.StylePriority.UseTextAlignment = false;
			this.xrLabel15.Text = "(Based on Voucher Date)";
			this.xrLabel15.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopCenter;
			// 
			// xrLine1
			// 
			this.xrLine1.BorderWidth = 2F;
			this.xrLine1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 81.8334F);
			this.xrLine1.Name = "xrLine1";
			this.xrLine1.SizeF = new System.Drawing.SizeF(801.4099F, 11.16657F);
			this.xrLine1.StylePriority.UseBorderWidth = false;
			// 
			// PageFooter
			// 
			this.PageFooter.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.pageInfo1,
            this.xrLabel21,
            this.pageInfo2,
            this.xrLabel25});
			this.PageFooter.Name = "PageFooter";
			// 
			// xrLabel25
			// 
			this.xrLabel25.BorderColor = System.Drawing.Color.Transparent;
			this.xrLabel25.Font = new DevExpress.Drawing.DXFont("Cambria", 10F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel25.ForeColor = System.Drawing.Color.DarkGray;
			this.xrLabel25.LocationFloat = new DevExpress.Utils.PointFloat(9.999998F, 9.999974F);
			this.xrLabel25.Name = "xrLabel25";
			this.xrLabel25.SizeF = new System.Drawing.SizeF(165.3535F, 20.66685F);
			this.xrLabel25.StylePriority.UseBorderColor = false;
			this.xrLabel25.StylePriority.UseFont = false;
			this.xrLabel25.StylePriority.UseForeColor = false;
			this.xrLabel25.Text = "Quickdice ERP Solutions";
			// 
			// pageInfo2
			// 
			this.pageInfo2.LocationFloat = new DevExpress.Utils.PointFloat(334.7917F, 7.666842F);
			this.pageInfo2.Name = "pageInfo2";
			this.pageInfo2.SizeF = new System.Drawing.SizeF(89.16663F, 23F);
			this.pageInfo2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.TopRight;
			this.pageInfo2.TextFormatString = "Page {0} of {1}";
			// 
			// xrLabel21
			// 
			this.xrLabel21.BorderColor = System.Drawing.Color.Transparent;
			this.xrLabel21.Font = new DevExpress.Drawing.DXFont("Arial", 8F, DevExpress.Drawing.DXFontStyle.Bold);
			this.xrLabel21.ForeColor = System.Drawing.Color.Black;
			this.xrLabel21.LocationFloat = new DevExpress.Utils.PointFloat(465.0632F, 6.833521F);
			this.xrLabel21.Name = "xrLabel21";
			this.xrLabel21.SizeF = new System.Drawing.SizeF(250.3535F, 15.49992F);
			this.xrLabel21.StylePriority.UseBorderColor = false;
			this.xrLabel21.StylePriority.UseFont = false;
			this.xrLabel21.StylePriority.UseForeColor = false;
			this.xrLabel21.Text = "Reported By:[Parameters.TenantName]";
			// 
			// pageInfo1
			// 
			this.pageInfo1.LocationFloat = new DevExpress.Utils.PointFloat(645.8333F, 7.666842F);
			this.pageInfo1.Name = "pageInfo1";
			this.pageInfo1.PageInfo = DevExpress.XtraPrinting.PageInfo.DateTime;
			this.pageInfo1.SizeF = new System.Drawing.SizeF(147.4999F, 14.66663F);
			// 
			// incomestatement
			// 
			this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail,
            this.GroupHeader1,
            this.GroupHeader2,
            this.ReportHeader,
            this.GroupHeader3,
            this.GroupFooter1,
            this.ReportFooter,
            this.GroupFooter2,
            this.PageFooter});
			this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
			this.DataMember = "sp20101IncomeStatement";
			this.DataSource = this.sqlDataSource1;
			this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
			this.Margins = new DevExpress.Drawing.DXMargins(25F, 20F, 25F, 25F);
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
		private DevExpress.XtraReports.UI.XRLabel xrLabel4;
		private DevExpress.XtraReports.UI.XRLabel xrLabel3;
		private DevExpress.XtraReports.UI.XRLabel xrLabel2;
		private DevExpress.XtraReports.UI.XRLabel xrLabel1;
		private DevExpress.XtraReports.UI.XRLabel xrLabel6;
		private DevExpress.XtraReports.UI.XRLabel xrLabel7;
		private DevExpress.XtraReports.UI.XRLabel xrLabel5;
		private DevExpress.XtraReports.UI.GroupHeaderBand GroupHeader3;
		private DevExpress.XtraReports.UI.XRLabel xrLabel8;
		private DevExpress.XtraReports.UI.GroupFooterBand GroupFooter1;
		private DevExpress.XtraReports.UI.ReportFooterBand ReportFooter;
		private DevExpress.XtraReports.UI.XRLabel xrLabel10;
		private DevExpress.XtraReports.UI.XRLabel xrLabel9;
		private DevExpress.XtraReports.UI.GroupFooterBand GroupFooter2;
		private DevExpress.XtraReports.UI.XRLabel xrLabel12;
		private DevExpress.XtraReports.UI.XRLabel xrLabel11;
		private DevExpress.XtraReports.UI.XRLabel xrLabel14;
		private DevExpress.XtraReports.UI.XRLabel xrLabel13;
		private DevExpress.XtraReports.UI.XRLabel xrLabel40;
		private DevExpress.XtraReports.UI.XRLabel xrLabel20;
		private DevExpress.XtraReports.UI.XRLabel xrLabel23;
		private DevExpress.XtraReports.UI.XRLabel xrLabel24;
		private DevExpress.XtraReports.UI.XRLine xrLine1;
		private DevExpress.XtraReports.UI.XRLabel xrLabel15;
		private DevExpress.XtraReports.UI.PageFooterBand PageFooter;
		private DevExpress.XtraReports.UI.XRLabel xrLabel25;
		private DevExpress.XtraReports.UI.XRPageInfo pageInfo2;
		private DevExpress.XtraReports.UI.XRLabel xrLabel21;
		private DevExpress.XtraReports.UI.XRPageInfo pageInfo1;
	}
}
