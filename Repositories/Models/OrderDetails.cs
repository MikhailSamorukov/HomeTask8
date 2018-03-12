using System.Collections.Generic;

namespace Repositories.Models
{
    public class OrderDetails
    {
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public decimal? UnitPrice { get; set; }
        public int? Quantity { get; set; }
        public double? Discount { get; set; }
    }
}