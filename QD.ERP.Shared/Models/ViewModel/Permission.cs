namespace QD.ERP.Shared.Models.ViewModel
{
    public class Permission
    {
        public int UserId { get; set; }
        public string ItemForm { get; set; }
        public string ItemName { get; set; }
        public bool? ItemEnabled { get; set; }
        public bool? ItemVisible { get; set; }
    }
}
