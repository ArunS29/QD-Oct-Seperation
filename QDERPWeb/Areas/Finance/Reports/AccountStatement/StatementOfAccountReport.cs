using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.Utils.Svg;
using DevExpress.XtraPrinting.Drawing;

namespace QD.ERP.Web.Reports
{
    public partial class StatementOfAccountReport : XtraReport
    {
        public StatementOfAccountReport(
            string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string company_Name, string company_address,
            Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }

        public StatementOfAccountReport()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate,
                                         string tenantName, string company_Name, string company_address,
                                         Image logoImage, string Company_Name_Ar, string company_address_arb)
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

            // Adding or updating report parameters
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));

            // Company & Tenant Information
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            Console.WriteLine($"Company Logo: {(logoImage != null ? "Exists" : "Not Provided")}");

            // Assigning text values to labels
            AssignLabelText("xrLabelTenantName", tenantName);
            AssignLabelText("xrLabelCompanyAddress", company_address);
            AssignLabelText("xrLabelCompanyNameAr", Company_Name_Ar);
            AssignLabelText("xrLabelCompanyAddressArb", company_address_arb);

            // Setting company logo
            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }

            // Apply icon formatting to relevant labels
            ApplyIconFormatting();
        }

        private void AssignLabelText(string controlName, string text)
        {
            if (this.FindControl(controlName, true) is XRLabel label)
            {
                label.Text = text ?? "";
            }
        }

        private void ApplyIconFormatting()
        {
            // Create a list to store labels with the "ShowIcon" tag
            List<XRLabel> labelsToModify = new List<XRLabel>();

            // Find all labels that need icons
            foreach (XRControl control in this.AllControls<XRControl>())
            {
                if (control is XRLabel label && label.Tag != null && label.Tag.ToString().ToLower() == "showicon")
                {
                    labelsToModify.Add(label);
                }
            }

            // Now add icons safely
            foreach (var label in labelsToModify)
            {
                AddSvgImageNextToLabel(label);
            }
        }


        private void AddSvgImageNextToLabel(XRLabel label)
        {
            string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string filePath = Path.Combine(webRootPath, "images", "sar 1.svg");

            SvgImage svg = SvgImage.FromFile(filePath);
            if (svg == null) return;

            // Define image size
            float iconWidth = 15;
            float iconHeight = 15;

            // Position the image next to the label (left side, centered vertically)
            float posX = label.LocationF.X - iconWidth - 5; // Small gap
            float posY = label.LocationF.Y + (label.HeightF - iconHeight) / 2;

            // Create an XRPictureBox to hold the SVG image
            XRPictureBox iconImage = new XRPictureBox
            {
                ImageSource = new ImageSource(svg),
                Sizing = ImageSizeMode.StretchImage,
                WidthF = iconWidth,
                HeightF = iconHeight,
                LocationF = new PointF(posX, posY),
                Borders = BorderSide.None
            };

            // Add the image next to the label
            label.Parent.Controls.Add(iconImage);
        }
    }
}
