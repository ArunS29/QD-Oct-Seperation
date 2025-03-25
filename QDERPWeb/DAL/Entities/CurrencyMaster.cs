using System;
using System.Collections.Generic;
namespace QD.ERP.Web.DAL.Entities
{
    public class CurrencyMaster
    {
        public int CurrencyID { get; set; }           // Unique Currency ID
        public string CurrencyName { get; set; }      // Currency Name (e.g., USD, EUR)
        public byte[] CurrencyLogo { get; set; }      // Currency Logo (byte array for storing images)
        public string CurrencySymbol { get; set; }    // Currency Symbol (e.g., $, €, ¥)
        public string CurrencyUnicode { get; set; }   // Unicode representation of the currency symbol
        public bool IsDefault { get; set; }           // Default currency (True = Yes, False = No)

    }
}
