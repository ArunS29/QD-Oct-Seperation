namespace QD.ERP.Web.Areas.Finance.Reports.AccountStatement
{
    partial class rpt201CostAllocationByAccountStatement
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
            DevExpress.DataAccess.Sql.SelectQuery selectQuery1 = new DevExpress.DataAccess.Sql.SelectQuery();
            DevExpress.DataAccess.Sql.AllColumns allColumns1 = new DevExpress.DataAccess.Sql.AllColumns();
            DevExpress.DataAccess.Sql.Table table1 = new DevExpress.DataAccess.Sql.Table();
            DevExpress.DataAccess.Sql.Column column1 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression1 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column2 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression2 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column3 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression3 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column4 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression4 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column5 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression5 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column6 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression6 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column7 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression7 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column8 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression8 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column9 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression9 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column10 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression10 = new DevExpress.DataAccess.Sql.ColumnExpression();
            DevExpress.DataAccess.Sql.Column column11 = new DevExpress.DataAccess.Sql.Column();
            DevExpress.DataAccess.Sql.ColumnExpression columnExpression11 = new DevExpress.DataAccess.Sql.ColumnExpression();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(rpt201CostAllocationByAccountStatement));
            this.TopMargin = new DevExpress.XtraReports.UI.TopMarginBand();
            this.BottomMargin = new DevExpress.XtraReports.UI.BottomMarginBand();
            this.Detail = new DevExpress.XtraReports.UI.DetailBand();
            this.XrTable1 = new DevExpress.XtraReports.UI.XRTable();
            this.tableCostAllocation = new DevExpress.XtraReports.UI.XRTableRow();
            this.XrTableCell3 = new DevExpress.XtraReports.UI.XRTableCell();
            this.XrTableCell6 = new DevExpress.XtraReports.UI.XRTableCell();
            this.XrTableCell1 = new DevExpress.XtraReports.UI.XRTableCell();
            this.XrTableCell2 = new DevExpress.XtraReports.UI.XRTableCell();
            this.VoucherEntryID = new DevExpress.XtraReports.Parameters.Parameter();
            this.sqlDataSource1 = new DevExpress.DataAccess.Sql.SqlDataSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.XrTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // TopMargin
            // 
            this.TopMargin.HeightF = 0F;
            this.TopMargin.Name = "TopMargin";
            // 
            // BottomMargin
            // 
            this.BottomMargin.HeightF = 0F;
            this.BottomMargin.Name = "BottomMargin";
            // 
            // Detail
            // 
            this.Detail.Controls.AddRange(new DevExpress.XtraReports.UI.XRControl[] {
            this.XrTable1});
            this.Detail.HeightF = 17F;
            this.Detail.Name = "Detail";
            // 
            // XrTable1
            // 
            this.XrTable1.Borders = DevExpress.XtraPrinting.BorderSide.Bottom;
            this.XrTable1.Font = new DevExpress.Drawing.DXFont("Microsoft Sans Serif", 9.75F, DevExpress.Drawing.DXFontStyle.Regular, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {
            new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
            this.XrTable1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.XrTable1.LocationFloat = new DevExpress.Utils.PointFloat(0F, 0F);
            this.XrTable1.Name = "XrTable1";
            this.XrTable1.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] {
            this.tableCostAllocation});
            this.XrTable1.SizeF = new System.Drawing.SizeF(558F, 17F);
            this.XrTable1.StylePriority.UseBorders = false;
            this.XrTable1.StylePriority.UseFont = false;
            this.XrTable1.StylePriority.UseForeColor = false;
            this.XrTable1.StylePriority.UseTextAlignment = false;
            this.XrTable1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            // 
            // tableCostAllocation
            // 
            this.tableCostAllocation.Cells.AddRange(new DevExpress.XtraReports.UI.XRTableCell[] {
            this.XrTableCell3,
            this.XrTableCell6,
            this.XrTableCell1,
            this.XrTableCell2});
            this.tableCostAllocation.Name = "tableCostAllocation";
            this.tableCostAllocation.Weight = 1D;
            // 
            // XrTableCell3
            // 
            this.XrTableCell3.BackColor = System.Drawing.Color.Gainsboro;
            this.XrTableCell3.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.XrTableCell3.Font = new DevExpress.Drawing.DXFont("Cambria", 8.25F, DevExpress.Drawing.DXFontStyle.Regular, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {
            new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
            this.XrTableCell3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.XrTableCell3.Multiline = true;
            this.XrTableCell3.Name = "XrTableCell3";
            this.XrTableCell3.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 2, 0, 0, 100F);
            this.XrTableCell3.ProcessDuplicatesMode = DevExpress.XtraReports.UI.ProcessDuplicatesMode.Merge;
            this.XrTableCell3.StylePriority.UseBackColor = false;
            this.XrTableCell3.StylePriority.UseBorders = false;
            this.XrTableCell3.StylePriority.UseFont = false;
            this.XrTableCell3.StylePriority.UseForeColor = false;
            this.XrTableCell3.StylePriority.UsePadding = false;
            this.XrTableCell3.StylePriority.UseTextAlignment = false;
            this.XrTableCell3.Text = "Cost Allocation:";
            this.XrTableCell3.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.XrTableCell3.Weight = 0.647035196187049D;
            // 
            // XrTableCell6
            // 
            this.XrTableCell6.BackColor = System.Drawing.Color.Transparent;
            this.XrTableCell6.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.XrTableCell6.CanGrow = false;
            this.XrTableCell6.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[CostAllocationUnit]")});
            this.XrTableCell6.Font = new DevExpress.Drawing.DXFont("Cambria", 8.25F, DevExpress.Drawing.DXFontStyle.Regular, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {
            new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
            this.XrTableCell6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.XrTableCell6.Name = "XrTableCell6";
            this.XrTableCell6.Padding = new DevExpress.XtraPrinting.PaddingInfo(3, 2, 0, 0, 100F);
            this.XrTableCell6.StylePriority.UseBackColor = false;
            this.XrTableCell6.StylePriority.UseBorders = false;
            this.XrTableCell6.StylePriority.UseFont = false;
            this.XrTableCell6.StylePriority.UseForeColor = false;
            this.XrTableCell6.StylePriority.UsePadding = false;
            this.XrTableCell6.StylePriority.UseTextAlignment = false;
            this.XrTableCell6.Text = "XrTableCell6";
            this.XrTableCell6.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
            this.XrTableCell6.Weight = 2.4299633116136157D;
            // 
            // XrTableCell1
            // 
            this.XrTableCell1.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.XrTableCell1.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[AmountAllocated]")});
            this.XrTableCell1.Font = new DevExpress.Drawing.DXFont("Cambria", 8.25F, DevExpress.Drawing.DXFontStyle.Italic);
            this.XrTableCell1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.XrTableCell1.Multiline = true;
            this.XrTableCell1.Name = "XrTableCell1";
            this.XrTableCell1.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 2, 0, 0, 100F);
            this.XrTableCell1.StylePriority.UseBorders = false;
            this.XrTableCell1.StylePriority.UseFont = false;
            this.XrTableCell1.StylePriority.UseForeColor = false;
            this.XrTableCell1.StylePriority.UsePadding = false;
            this.XrTableCell1.StylePriority.UseTextAlignment = false;
            this.XrTableCell1.Text = "XrTableCell1";
            this.XrTableCell1.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.XrTableCell1.TextFormatString = "{0:n2}";
            this.XrTableCell1.Weight = 0.81105665734405019D;
            // 
            // XrTableCell2
            // 
            this.XrTableCell2.Borders = DevExpress.XtraPrinting.BorderSide.None;
            this.XrTableCell2.ExpressionBindings.AddRange(new DevExpress.XtraReports.UI.ExpressionBinding[] {
            new DevExpress.XtraReports.UI.ExpressionBinding("BeforePrint", "Text", "[CostAllocDrCr]")});
            this.XrTableCell2.Font = new DevExpress.Drawing.DXFont("Cambria", 8.25F, DevExpress.Drawing.DXFontStyle.Regular, DevExpress.Drawing.DXGraphicsUnit.Point, new DevExpress.Drawing.DXFontAdditionalProperty[] {
            new DevExpress.Drawing.DXFontAdditionalProperty("GdiCharSet", ((byte)(0)))});
            this.XrTableCell2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.XrTableCell2.Multiline = true;
            this.XrTableCell2.Name = "XrTableCell2";
            this.XrTableCell2.Padding = new DevExpress.XtraPrinting.PaddingInfo(0, 2, 0, 0, 100F);
            this.XrTableCell2.StylePriority.UseBorders = false;
            this.XrTableCell2.StylePriority.UseFont = false;
            this.XrTableCell2.StylePriority.UseForeColor = false;
            this.XrTableCell2.StylePriority.UsePadding = false;
            this.XrTableCell2.StylePriority.UseTextAlignment = false;
            this.XrTableCell2.Text = "XrTableCell2";
            this.XrTableCell2.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleRight;
            this.XrTableCell2.Weight = 0.35954065894373838D;
            // 
            // VoucherEntryID
            // 
            this.VoucherEntryID.Name = "VoucherEntryID";
            this.VoucherEntryID.Type = typeof(int);
            this.VoucherEntryID.ValueInfo = "0";
            this.VoucherEntryID.Visible = false;
            // 
            // sqlDataSource1
            // 
            this.sqlDataSource1.ConnectionName = "DBConnection";
            this.sqlDataSource1.Name = "sqlDataSource1";
            table1.MetaSerializable = "<Meta X=\"30\" Y=\"30\" Width=\"125\" Height=\"284\" />";
            table1.Name = "qry201RptVoucherWithCost";
            allColumns1.Table = table1;
            columnExpression1.ColumnName = "CostAllocationID";
            columnExpression1.Table = table1;
            column1.Expression = columnExpression1;
            columnExpression2.ColumnName = "VoucherEntryID";
            columnExpression2.Table = table1;
            column2.Expression = columnExpression2;
            columnExpression3.ColumnName = "CostAllocationUnit";
            columnExpression3.Table = table1;
            column3.Expression = columnExpression3;
            columnExpression4.ColumnName = "CostAllocationGroup";
            columnExpression4.Table = table1;
            column4.Expression = columnExpression4;
            columnExpression5.ColumnName = "CostAllocDrCr";
            columnExpression5.Table = table1;
            column5.Expression = columnExpression5;
            columnExpression6.ColumnName = "AmountAllocated";
            columnExpression6.Table = table1;
            column6.Expression = columnExpression6;
            columnExpression7.ColumnName = "CostAllocRemarks";
            columnExpression7.Table = table1;
            column7.Expression = columnExpression7;
            columnExpression8.ColumnName = "AccountHeadName";
            columnExpression8.Table = table1;
            column8.Expression = columnExpression8;
            columnExpression9.ColumnName = "VoucherNo";
            columnExpression9.Table = table1;
            column9.Expression = columnExpression9;
            columnExpression10.ColumnName = "VoucherAmountFormatted";
            columnExpression10.Table = table1;
            column10.Expression = columnExpression10;
            columnExpression11.ColumnName = "CurrencyRate";
            columnExpression11.Table = table1;
            column11.Expression = columnExpression11;
            selectQuery1.Columns.Add(allColumns1);
            selectQuery1.Columns.Add(column1);
            selectQuery1.Columns.Add(column2);
            selectQuery1.Columns.Add(column3);
            selectQuery1.Columns.Add(column4);
            selectQuery1.Columns.Add(column5);
            selectQuery1.Columns.Add(column6);
            selectQuery1.Columns.Add(column7);
            selectQuery1.Columns.Add(column8);
            selectQuery1.Columns.Add(column9);
            selectQuery1.Columns.Add(column10);
            selectQuery1.Columns.Add(column11);
            selectQuery1.Name = "qry201RptVoucherWithCost";
            selectQuery1.Tables.Add(table1);
            this.sqlDataSource1.Queries.AddRange(new DevExpress.DataAccess.Sql.SqlQuery[] {
            selectQuery1});
            this.sqlDataSource1.ResultSchemaSerializable = resources.GetString("sqlDataSource1.ResultSchemaSerializable");
            // 
            // rpt201CostAllocationByAccountStatement
            // 
            this.Bands.AddRange(new DevExpress.XtraReports.UI.Band[] {
            this.TopMargin,
            this.BottomMargin,
            this.Detail});
            this.ComponentStorage.AddRange(new System.ComponentModel.IComponent[] {
            this.sqlDataSource1});
            this.DataMember = "qry201RptVoucherWithCost";
            this.DataSource = this.sqlDataSource1;
            this.FilterString = "[VoucherEntryID] = ?VoucherEntryID";
            this.Font = new DevExpress.Drawing.DXFont("Arial", 9.75F);
            this.Margins = new DevExpress.Drawing.DXMargins(0F, 262F, 0F, 0F);
            this.PageHeight = 1169;
            this.PageWidth = 827;
            this.PaperKind = DevExpress.Drawing.Printing.DXPaperKind.A4;
            this.Parameters.AddRange(new DevExpress.XtraReports.Parameters.Parameter[] {
            this.VoucherEntryID});
            this.Version = "24.1";
            ((System.ComponentModel.ISupportInitialize)(this.XrTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        #endregion

        private DevExpress.XtraReports.UI.TopMarginBand TopMargin;
        private DevExpress.XtraReports.UI.BottomMarginBand BottomMargin;
        private DevExpress.XtraReports.UI.DetailBand Detail;
        private DevExpress.XtraReports.UI.XRTable XrTable1;
        private DevExpress.XtraReports.UI.XRTableRow tableCostAllocation;
        private DevExpress.XtraReports.UI.XRTableCell XrTableCell3;
        private DevExpress.XtraReports.UI.XRTableCell XrTableCell6;
        private DevExpress.XtraReports.UI.XRTableCell XrTableCell1;
        private DevExpress.XtraReports.UI.XRTableCell XrTableCell2;
        private DevExpress.XtraReports.Parameters.Parameter VoucherEntryID;
        private DevExpress.DataAccess.Sql.SqlDataSource sqlDataSource1;
    }
}
