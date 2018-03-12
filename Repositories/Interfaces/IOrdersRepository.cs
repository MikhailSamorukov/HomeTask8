using System;
using System.Collections.Generic;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IOrdersRepository
    {
        IEnumerable<Order> GetOrders();
        IEnumerable<OrderHistory> CallCustOrderHist(string customerId);
        IEnumerable<CustomerOrdersDetail> CallCustOrdersDetail(int orderId);
        void RemoveInProggressAndNewOrders();
        void SetShippedDate(Order order, DateTime? orderDate);
        void SetOrderDate(Order order, DateTime? shippedDate);
        void AddOrder(Order order);
        Order GetOrderWithDetailsById(int orderId);
        void UpdateOrder(Order order);
        int? GetLastOrderId();
    }
}