using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using Svg;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;

namespace QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices
    
{
   	public partial class ForeignCurrencyProformaWithoutDiscount : DevExpress.XtraReports.UI.XtraReport
{
    //public ForeignCurrencyProforma()
    //{
    //	InitializeComponent();
    //}
    private readonly TenantDbContextHelper _tenantDbContextHelper;
    private bool _isApproved;

    public ForeignCurrencyProformaWithoutDiscount(
        string invoiceNo,
        string tenantName,
        string companyName,
        string companyAddress,
        Image logoImage,
        Image sealImage,
        string companyNameAr,
        string companyAddressAr,
          string companyPhone,
        string companyEmail,
        string companyWebsite,
        bool isApproved,
        TenantDbContextHelper tenantDbContextHelper)
    {
        _tenantDbContextHelper = tenantDbContextHelper;
        _isApproved = isApproved;

        InitializeComponent();
        SetReportParameters(invoiceNo, tenantName, companyName, companyAddress, logoImage, sealImage, companyNameAr, companyAddressAr, companyPhone, companyEmail, companyWebsite);
        LoadReportData(invoiceNo);
        LoadCurrencySymbolAndImage();
    }

    private void SetReportParameters(string invoiceNo, string tenantName, string companyName, string companyAddress, Image logoImage, Image sealImage, string companyNameAr, string companyAddressAr, string companyPhone, string companyEmail, string companyWebsite)
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

        AddOrUpdateParameter("InvoiceNo", invoiceNo, typeof(string));
        AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
        AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
        AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
        AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
        AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
        AddOrUpdateParameter("CompanyPhone", companyPhone ?? "", typeof(string));
        AddOrUpdateParameter("CompanyEmailAddress", companyEmail ?? "", typeof(string));

        AddOrUpdateParameter("CompanyWebsite", companyWebsite ?? "", typeof(string));

        if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            tenantLabel.Text = tenantName;

        if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
            companyNameLabel.Text = companyName;

        if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            addressLabel.Text = companyAddress;

        if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            companyNameArLabel.Text = companyNameAr;

        if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
            addressArLabel.Text = companyAddressAr;


        if (FindControl("xrPictureBox2", true) is XRPictureBox logoPictureBox)
            logoPictureBox.Image = logoImage;
        if (FindControl("xrPictureBox1", true) is XRPictureBox sealPictureBox)
            sealPictureBox.Image = sealImage;
    }

    private void LoadReportData(string invoiceNo)
    {
        DataTable dt = GetReportData(invoiceNo);

        if (dt.Rows.Count == 0)
        {
            this.DataSource = null;

        }
        else
        {
            this.DataSource = dt;
            this.DataMember = "";
            SetWatermark();
        }
    }

    private DataTable GetReportData(string invoiceNo)
    {
        DataTable dt = new DataTable();

        try
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM qry201_652ProformaInvoiceReport WHERE ProformaInvoiceNo  = @ProformaInvoiceNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@ProformaInvoiceNo", invoiceNo);

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

    private void SetWatermark()
    {
        if (!_isApproved)
        {
            this.Watermark.Text = "DRAFT COPY";
            this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);
            this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230);
            this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
            this.Watermark.ShowBehind = true;
            this.Watermark.ImageTiling = false;
            this.Watermark.ImageViewMode = ImageViewMode.Stretch;
        }
    }

    private void LoadCurrencySymbolAndImage()
    {
        try
        {
            if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                SetCurrencyImageNull();
                return;
            }

            string connectionString = tenant.ConnectionString;
            string svgText = null;
            string currencySymbol = null;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = $@"
                        SELECT c.CurrencyImage, c.CurrencySymbol
                        FROM {tenant.schemaname}.tbl901CompanyDetails AS c
                        INNER JOIN dbo.fn_GetDefaultCompanyDetails() AS f ON c.CompanyId = f.CompanyId";

                using (var command = new SqlCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        svgText = reader["CurrencyImage"]?.ToString()?.Trim('\uFEFF');
                        currencySymbol = reader["CurrencySymbol"]?.ToString()?.Trim();
                    }
                }
            }

            if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel && !string.IsNullOrEmpty(currencySymbol))
                currencyLabel.Text = currencySymbol;

            if (string.IsNullOrWhiteSpace(svgText))
            {
                SetCurrencyImageNull();
                return;
            }

            Bitmap bitmap = null;
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText)))
                {
                    SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(stream);
                    bitmap = svgDoc.Draw();
                }
            }
            catch { bitmap = null; }

            if (bitmap == null)
            {
                SetCurrencyImageNull();
                return;
            }

            string[] pictureBoxNames = {  "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = bitmap;
                    pictureBox.Sizing = ImageSizeMode.Normal;
                }
            }
            AlignCurrencyWithAmount(bitmap);
        }
        catch
        {
            SetCurrencyImageNull();
        }
    }

    private void SetCurrencyImageNull()
    {
        string[] pictureBoxNames = {  "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

        foreach (string name in pictureBoxNames)
        {
            if (FindControl(name, true) is XRPictureBox pictureBox)
            {
                pictureBox.Image = null;
                pictureBox.ImageSource = null;
            }
        }

        if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel)
            currencyLabel.Text = "";
    }

    private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
    {
        var fixedPictureBoxes = new[] { "xrPictureBox4", "xrPictureBox3", "xrPictureBox5", "xrPictureBox6" ,
                "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox11", "xrPictureBox12" };
        foreach (var name in fixedPictureBoxes)
        {
            if (FindControl(name, true) is XRPictureBox picBox)
            {
                picBox.Image = bitmap;
                picBox.Sizing = ImageSizeMode.StretchImage;
                picBox.WidthF = iconSize;
                picBox.HeightF = iconSize;
            }
        }
        var pairs = new[]
        {


    //new { Label = "xrLabel5",  Picture = "xrPictureBox2" },
    //new { Label = "xrLabel7",  Picture = "xrPictureBox4" },
    new { Label = "xrLabel0",  Picture = "xrPictureBox0" },

};

        foreach (var p in pairs)
        {
            var label = FindControl(p.Label, true) as XRLabel;
            var pictureBox = FindControl(p.Picture, true) as XRPictureBox;

            if (label == null || pictureBox == null)
                continue;

            pictureBox.Image = bitmap;
            pictureBox.Sizing = ImageSizeMode.StretchImage;

            label.BeforePrint += (s, e) =>
            {
                var lbl = (XRLabel)s;

                using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
                {
                    float iconHeight = lbl.Font.Size + 0.2f;// Match icon to font height
                    float iconWidth = iconHeight;            // Keep square

                    pictureBox.WidthF = iconWidth;
                    pictureBox.HeightF = iconHeight;

                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

                    var format = StringFormat.GenericTypographic;
                    format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                    float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;
                    float spaceWidth = g.MeasureString(" ", sysFont).Width;

                    float rightEdge = lbl.LocationF.X + lbl.WidthF;
                    float posX = rightEdge - textWidth - iconWidth - 5f - spaceWidth;

                    pictureBox.LocationF = new PointF(posX, posY);
                }
            };
        }
    }


}
}
