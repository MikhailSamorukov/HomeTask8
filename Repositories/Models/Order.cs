using System;
using System.Collections.Generic;
using System.Text;
using Repositories.Enums;

namespace Repositories.Models
{
    public class Order
    {
        public int OrderId { get; }
        public string CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime? OrderDate { get; private set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? ShippedDate { get; private set; }
        public int? ShipVia { get; set; }
        public decimal? Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipAddress { get; set; }
        public string ShipCity { get; set; }
        public string ShipRegion { get; set; }
        public string ShipPostalCode { get; set; }
        public string ShipCountry { get; set; }
        public List<OrderDetails> Details { get; set; }
        public List<Product> Products { get; set; }

        public OrderStatus Status
        {
            get
            {
                if (!OrderDate.HasValue)
                    return OrderStatus.New;

                return !ShippedDate.HasValue ? OrderStatus.InProgress : OrderStatus.Completed;
            }
        }

        public Order()
        {
        }

        internal Order(int orderId) => OrderId = orderId;

        internal void SetOrderDate(DateTime? newDate) => OrderDate = newDate;

        internal void SetShippedDate(DateTime? newDate) => ShippedDate = newDate;
    }
}
