using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Shared.DAL.Entities
{
    [Keyless]
    public class Qry01Bankandcashbalance
    {
        public string AccountHeadName { get; set; }
        public string AccountGroup { get; set; }
        public decimal Balance { get; set; }
    }
}
