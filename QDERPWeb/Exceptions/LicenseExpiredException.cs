using System;

namespace QD.ERP.Web.Exceptions
{
    public class LicenseExpiredException : Exception
    {
        public LicenseExpiredException(string message)
            : base(message)
        {
        }
    }
}
