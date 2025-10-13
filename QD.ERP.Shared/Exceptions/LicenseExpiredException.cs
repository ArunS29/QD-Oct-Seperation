using System;

namespace QD.ERP.Shared.Exceptions
{
    public class LicenseExpiredException : Exception
    {
        public LicenseExpiredException(string message)
            : base(message)
        {
        }
    }
}
