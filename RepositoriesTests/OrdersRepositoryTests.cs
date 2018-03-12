using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Repositories;
using Repositories.Interfaces;
using Repositories.Models;

namespace RepositoriesTests
{
    [TestFixture]
    public class OrdersRepositoryTests
    {
        private IOrdersRepository _repo;

        [SetUp]
        public void Init()
        {
            var connection = System.Configuration.ConfigurationManager.ConnectionStrings["Northwind"];
            _repo = new OrdersRepository(connection.ConnectionString,connection.ProviderName);
        }

        [Test]
        [Category("Integration")]
        public void Should_delete_all_orders_InProggress_And_New_Or_Throw_Exception()
        {
            //Arrange
            //Act
            var exception = Assert.Throws<Exception> (() => _repo.RemoveInProggressAndNewOrders());
            //Assert
            Assert.IsTrue(!_repo
                .GetOrders()
                .Any(order =>
                    order.ShippedDate == null ||order.OrderDate ==null) || 
                    exception.Message.Contains("Conflict with foreign key, see inner details"));
        }
        [Test]
        [Category("Integration")]
        public void Should_insert_new_order_and_select_it_by_id()
        {
            //Arrange
            var order = new Order()
            {
                CustomerId = "ALFKI",
                EmployeeId = 1,
                ShipVia = 1,
                ShipName = "test"
            };
            //Act
            _repo.AddOrder(order);
            var selectedOrder = _repo.GetOrderWithDetailsById((int)_repo.GetLastOrderId());
            //Assert
            Assert.IsTrue(selectedOrder.ShipName == order.ShipName 
                          && order.CustomerId == selectedOrder.CustomerId 
                          && order.EmployeeId == selectedOrder.EmployeeId
                          && order.ShipVia == selectedOrder.ShipVia);
        }
        [Test]
        [Category("Integration")]
        public void Should_update_OrderDate_to_Current()
        {
            //Arrange
            var lastId = (int)_repo.GetLastOrderId();
            var selectedOrder = _repo.GetOrderWithDetailsById(lastId);
            //Act
            _repo.SetShippedDate(selectedOrder, DateTime.Now.Date);
            var orderAfterUpdate = _repo.GetOrderWithDetailsById(lastId);
            //Assert
            Assert.IsTrue(orderAfterUpdate.OrderDate == DateTime.Now.Date);
        }

        [Test]
        [Category("Integration")]
        public void Should_update_ShippedDate_to_Current()
        {
            //Arrange
            var lastId = (int)_repo.GetLastOrderId();
            var selectedOrder = _repo.GetOrderWithDetailsById(lastId);
            //Act
            _repo.SetOrderDate(selectedOrder, DateTime.Now.Date);
            var orderAfterUpdate = _repo.GetOrderWithDetailsById(lastId);
            //Assert
            Assert.IsTrue(orderAfterUpdate.OrderDate == DateTime.Now.Date);
        }

        [Test]
        [Category("Integration")]
        public void Should_throw_exception_before_update()
        {
            //Arrange
            var lastId = (int)_repo.GetLastOrderId();
            var selectedOrder = _repo.GetOrderWithDetailsById(lastId);
            //Act
            _repo.SetOrderDate(selectedOrder, DateTime.Now.Date);
            var exception = Assert.Throws<Exception>(() => _repo.UpdateOrder(selectedOrder));
            //Assert
            Assert.IsTrue(exception.Message == "order should be in status new");
        }
        [Test]
        [Category("Integration")]
        public void Should_update_some_fields()
        {
            const string UPDATED_TEXT = "Tes test test";
            //Arrange
            var lastId = (int)_repo.GetLastOrderId();
            var selectedOrder = _repo.GetOrderWithDetailsById(lastId);
            //Act
            _repo.SetOrderDate(selectedOrder, null);
            _repo.SetShippedDate(selectedOrder, null);
            var clearedOrder = _repo.GetOrderWithDetailsById(lastId);
            clearedOrder.ShipName = UPDATED_TEXT;
            clearedOrder.ShipAddress = UPDATED_TEXT;
            clearedOrder.ShipCity = UPDATED_TEXT;
            _repo.UpdateOrder(clearedOrder);
            var upatedOrder = _repo.GetOrderWithDetailsById(lastId);
            //Assert
            Assert.IsTrue(upatedOrder.ShipName == UPDATED_TEXT &&
                          upatedOrder.ShipAddress == UPDATED_TEXT &&
                          upatedOrder.ShipCity == UPDATED_TEXT);
        }
        [Test]
        [Category("Manual")]
        public void Should_call_CustOrderHist()
        {
            var ordersHistory = _repo.CallCustOrderHist("ALFKI");
            Assert.IsTrue(ordersHistory.Any());
        }

        [Test]
        [Category("Manual")]
        public void Should_call_CustOrdersDetail()
        {
            var custOrdersDetail = _repo.CallCustOrdersDetail(10248);
            Assert.IsTrue(custOrdersDetail.Any());
        }
    }
}
