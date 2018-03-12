using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Repositories.Abstract;
using Repositories.Enums;
using Repositories.Helpers;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories
{
    public class OrdersRepository : AbstractRepository, IOrdersRepository
    {
        public OrdersRepository(string connectionString, string provider)
            : base(connectionString, provider)
        {
        }

        public IEnumerable<Order> GetOrders()
        {
            var resultOrders = new List<Order>();
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from dbo.orders";
                command.CommandType = CommandType.Text;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var order = ReadOrder(reader);
                        resultOrders.Add(order);
                    }
                }
            }

            connection.Close();
            return resultOrders;
        }

        public IEnumerable<OrderHistory> CallCustOrderHist(string customerId)
        {
            var resultOrdersHistory = new List<OrderHistory>();
            var connection = GetConnection();
            var command = GetExecProcedureCommand(connection, "CustOrderHist", "@CustomerID", customerId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var order = ReadOrderHistory(reader);
                    resultOrdersHistory.Add(order);
                }
            }

            connection.Close();
            return resultOrdersHistory;
        }

        public IEnumerable<CustomerOrdersDetail> CallCustOrdersDetail(int orderId)
        {
            var resultCustomerOrdersDetail = new List<CustomerOrdersDetail>();
            var connection = GetConnection();
            var command = GetExecProcedureCommand(connection, "CustOrdersDetail", "@OrderID", orderId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var order = ReadCustomerOrdersDetail(reader);
                    resultCustomerOrdersDetail.Add(order);
                }
            }

            connection.Close();
            return resultCustomerOrdersDetail;
        }

        public void RemoveInProggressAndNewOrders()
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "delete from dbo.Orders where OrderDate is null or ShippedDate is null";
                command.CommandType = CommandType.Text;
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    throw new Exception($"Conflict with foreign key, see inner details: {ex.Message}");
                }
            }

            connection.Close();
        }

        public void SetShippedDate(Order order, DateTime? orderDate)
        {
            order.SetOrderDate(orderDate);
            UpdateFieldByOrderId(order, "OrderDate", order.OrderDate);
        }

        public void SetOrderDate(Order order, DateTime? shippedDate)
        {
            order.SetShippedDate(shippedDate);
            UpdateFieldByOrderId(order, "ShippedDate", order.ShippedDate);
        }

        public void AddOrder(Order order)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "insert into dbo.orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, ShipVia, Freight, " +
                                      "ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry) " +
                                      "values(@CustomerId, @EmployeeId, @OrderDate, @RequiredDate, @ShippedDate, @ShipVia, @Freight, " +
                                      "@ShipName, @ShipAddress, @ShipCity, @ShipRegion, @ShipPostalCode, @ShipCountry)";
                command.CommandType = CommandType.Text;

                SetOrderParameters(order, command);
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        public int? GetLastOrderId()
        {
            var connection = GetConnection();
            int? id;
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT IDENT_CURRENT(\'Orders\') Id";
                id = command.ExecuteScalar().ToInt();
            }
            connection.Close();
            return id;
        }

        public Order GetOrderWithDetailsById(int orderId)
        {
            Order order;
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "select * from dbo.orders where OrderID = @Id;" +
                                      "select * from dbo.[Order Details] where OrderID = @Id;" +
                                      @"select dbo.Products.ProductID, ProductName from dbo.orders
                                        join dbo.[Order Details] on dbo.orders.OrderID = dbo.[Order Details].OrderID
                                        join dbo.Products on dbo.[Order Details].ProductID = dbo.Products.ProductID
                                        where dbo.orders.OrderID = @Id;";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(CreateParameter(command, "@Id", orderId));
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return null;
                    reader.Read();
                    order = ReadOrder(reader);
                    reader.NextResult();
                    ReadOrderDetails(order, reader);
                    reader.NextResult();
                    ReadProducts(order, reader);
                }
            }

            connection.Close();
            return order;
        }

        public void UpdateOrder(Order order)
        {
            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.InProgress)
                throw new Exception("order should be in status new");

            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE dbo.orders SET " +
                                      "CustomerId = @CustomerId, " +
                                      "OrderDate = @OrderDate, " +
                                      "ShippedDate = @ShippedDate, " +
                                      "EmployeeId = @EmployeeId, " +
                                      "ShipVia = @ShipVia, " +
                                      "Freight = @Freight, " +
                                      "ShipName = @ShipName, " +
                                      "ShipAddress = @ShipAddress, " +
                                      "ShipCity = @ShipCity, " +
                                      "ShipRegion = @ShipRegion, " +
                                      "ShipPostalCode = @ShipPostalCode, " +
                                      "ShipCountry = @ShipCountry " +
                                      " WHERE OrderId= @OrderId";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(CreateParameter(command, "@OrderId", order.OrderId));
                SetOrderParameters(order, command);
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        private void UpdateFieldByOrderId(Order order, string param, object value)
        {
            var connection = GetConnection();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE dbo.orders SET " +
                                      $"{param} = @{param} " +
                                      " WHERE OrderId= @OrderID";
                command.CommandType = CommandType.Text;
                command.Parameters.Add(CreateParameter(command, "@OrderID", order.OrderId));
                command.Parameters.Add(CreateParameter(command, $"@{param}", value ?? DBNull.Value));

                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        private static DbCommand GetExecProcedureCommand(DbConnection connection, string commandText, string parameterAlias,
            object parameterValue)
        {
            var command = connection.CreateCommand();

            command.CommandText = commandText;
            command.Parameters.Add(CreateParameter(command, parameterAlias, parameterValue));
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

        private static void SetOrderParameters(Order order, DbCommand command)
        {
            command.Parameters.Add(CreateParameter(command, "@CustomerId", order.CustomerId));
            command.Parameters.Add(CreateParameter(command, "@EmployeeId", order.EmployeeId));
            command.Parameters.Add(CreateParameter(command, "@OrderDate", order.OrderDate));
            command.Parameters.Add(CreateParameter(command, "@RequiredDate", order.RequiredDate));
            command.Parameters.Add(CreateParameter(command, "@ShippedDate", order.ShippedDate));
            command.Parameters.Add(CreateParameter(command, "@ShipVia", order.ShipVia));
            command.Parameters.Add(CreateParameter(command, "@Freight", order.Freight));
            command.Parameters.Add(CreateParameter(command, "@ShipName", order.ShipName));
            command.Parameters.Add(CreateParameter(command, "@ShipAddress", order.ShipAddress));
            command.Parameters.Add(CreateParameter(command, "@ShipCity", order.ShipCity));
            command.Parameters.Add(CreateParameter(command, "@ShipRegion", order.ShipRegion));
            command.Parameters.Add(CreateParameter(command, "@ShipPostalCode", order.ShipPostalCode));
            command.Parameters.Add(CreateParameter(command, "@ShipCountry", order.ShipCountry));
        }

        private static void ReadProducts(Order order, IDataReader reader)
        {
            while (reader.Read())
            {
                order.Products = new List<Product>()
                {
                    new Product()
                    {
                        ProductId = reader.GetInt32(0),
                        ProductName = reader.GetString(1)
                    }
                };
            }
        }

        private static void ReadOrderDetails(Order order, IDataReader reader)
        {
            while (reader.Read())
            {
                order.Details = new List<OrderDetails>()
                {
                    new OrderDetails
                    {
                        OrderId = reader.GetInt32(0),
                        ProductId = reader.GetInt32(1),
                        UnitPrice = reader.GetDecimal(2),
                        Quantity = reader.GetInt32(3),
                        Discount = reader.GetDouble(4),
                    }
                };
            }
        }

        private OrderHistory ReadOrderHistory(IDataRecord reader)
        {
            var orderHistory = new OrderHistory()
            {
                ProductName = reader.GetString(0),
                Total = reader.GetInt32(1),

            };
            return orderHistory;
        }

        private static CustomerOrdersDetail ReadCustomerOrdersDetail(IDataRecord reader)
        {
            var customerOrdersDetail = new CustomerOrdersDetail()
            {
                ProductName = reader[0].ToString(),
                UnitPrice = reader[1].ToDecimal(),
                Quantity = reader[2].ToInt(),
                Discount = reader[3].ToDouble(),
                ExtendedPrice = reader[4].ToDouble(),
            };
            return customerOrdersDetail;
        }

        private Order ReadOrder(DbDataReader reader)
        {
            var order = new Order(reader.GetInt32(0))
            {
                CustomerId = reader.GetString(1),
                EmployeeId = reader[2]?.ToInt(),
                RequiredDate = reader[4]?.ToDateTime(),
                ShipVia = reader[6]?.ToInt(),
                Freight = reader[7]?.ToDecimal(),
                ShipName = reader[8].ToString(),
                ShipAddress = reader[9].ToString(),
                ShipCity = reader[10].ToString(),
                ShipRegion = reader[11].ToString(),
                ShipPostalCode = reader[12].ToString(),
                ShipCountry = reader[13].ToString(),
            };
            order.SetOrderDate(reader[3]?.ToDateTime());
            order.SetShippedDate(reader[5]?.ToDateTime());
            return order;
        }

        private static DbParameter CreateParameter(DbCommand command, string alias, object value)
        {
            var parametr = command.CreateParameter();
            parametr.ParameterName = alias;
            parametr.Value = value ?? "";
            return parametr;
        }
    }
}
