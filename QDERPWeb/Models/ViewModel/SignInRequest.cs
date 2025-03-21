namespace QD.ERP.Web.Models.ViewModel
{
    public class SignInRequest
    {
        //   public string userId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenantName { get; set; }
        public Boolean ResetPassword { get; set; }
        public string otp { get; set; }


    }
}
