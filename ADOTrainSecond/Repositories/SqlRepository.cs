using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using ADOTrainSecond.Dtos;

namespace ADOTrainSecond.Repositories
{
    public class SqlRepository:BaseRepository
    {
        private const string GetOrdersQuery = "Select * From Orders";
        private const string GetOrderProcedure = "GetOrder";
        private const string CustOrdersDetailProcedure = "CustOrdersDetail";
        private const string CreateNewOrderQuery = "Insert into Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, ShipVia, Freight, ShipName, ShipAddress, ShipCity, ShipRegion, ShipPostalCode, ShipCountry) " +
                                                                        "Values @customerId, @employeeId, @orderDate, @requiredDate, @shippedDate, @shipVia, @freight, @shipName, @shipAddress, @shipCity, @shipRegion, @shipPostalCode, @shipCountry";
        private const string UpdateOrderQuery = "Update Orders Set CustomerID = @customerId, EmployeeID = @employeeId, RequiredDate = @requiredDate, ShipVia = @shipVia, Freight = @freight, ShipName = @shipName, ShipAddress = @shipAddress, ShipCity = @shipCity, ShipRegion = @shipRegion, ShipPostalCode = @shipPostalCode, ShipCountry = @shipCountry Where OrderId = @orderId";
        private const string GetOrderByIdQuery = "Select * From Orders Where OrderId = @orderId";
        private const string DeleteNewAndInWorkOrdersQuery = "Delete From Orders Where OrderDate = null or ShippedDate = null";
        private const string UpdateOrderDateQuery = "Update Orders Set OrderDate = @orderDate Where OrderId = @orderId";
        private const string UpdateShippedDateQuery = "Update Orders Set ShippedDate = @shippedDate Where OrderId = @orderId";
        private const string CustOrderHistStoredProcedure = "CustOrderHist";
        protected override T GetConnection<T>(Func<IDbConnection, T> getData, string providerInvariantName = default, string connectionString = default)
        {
            providerInvariantName = Properties.Resource.NorthwindProviderName;
            connectionString = Properties.Resource.NorthwindConnectionString;
            return base.GetConnection(getData, providerInvariantName, connectionString);
        }

        private static void CreateSqlParameter(SqlCommand command, string parameterName, SqlDbType sqlDbType = default,
            int size = -1, ParameterDirection direction = default, object value = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.SqlDbType = sqlDbType;
            parameter.Direction = direction;
            parameter.Value = value;

            if (size > 0)
            {
                parameter.Size = size;
            }

            command.Parameters.Add(parameter);
        }

        public IEnumerable<Order> GetOrders()
        {
            return GetConnection(con =>
            {
                var result = new List<Order>();
                using (var command = CreateCommand<SqlCommand>(con, GetOrdersQuery))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[0] is int orderId) || !(reader[2] is int employeeId) || !(reader[6] is int shipVia) || !(reader[7] is decimal freight))
                            {
                                continue;
                            }

                            var ord = new Order
                            {
                                OrderId = orderId,
                                CustomerId = reader[1].ToString(),
                                EmployeeId = employeeId,
                                OrderDate = reader[3] as DateTime?,
                                RequiredDate = reader[4] as DateTime?,
                                ShippedDate = reader[5] as DateTime?,
                                ShipVia = shipVia,
                                Freight = freight,
                                ShipName = reader[8].ToString(),
                                ShipAddress = reader[9].ToString(),
                                ShipCity = reader[10].ToString(),
                                ShipRegion = reader[11].ToString(),
                                ShipPostalCode = reader[12].ToString(),
                                ShipCountry = reader[13].ToString()
                            };

                            if (ord.OrderDate == null)
                            {
                                ord.OrderState = OrderState.New;
                            }
                            else if (ord.ShippedDate == null)
                            {
                                ord.OrderState = OrderState.InWork;
                            }
                            else
                            {
                                ord.OrderState = OrderState.Done;
                            }

                            result.Add(ord);
                        }
                    }
                }
                return result;
            });
        }

        public Order GetDetailedOrder(int orderId)
        {
            return GetConnection(con =>
            {
                var order = new Order();

                using (var command =
                    CreateCommand<SqlCommand>(con, GetOrderProcedure, commandType: CommandType.StoredProcedure))
                {
                    CreateSqlParameter(command, "@OrderId", SqlDbType.Int, direction: ParameterDirection.Input,
                        value: orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!(reader[0] is int ordId) || !(reader[2] is int employeeId) ||
                                !(reader[6] is int shipVia) || !(reader[7] is decimal freight))
                            {
                                throw new ArgumentException(
                                    $"SqlTypes doesn't match with CLR types.\nStoredProcedure: {GetOrderProcedure}");
                            }

                            order = new Order
                            {
                                OrderId = ordId,
                                CustomerId = reader[1].ToString(),
                                EmployeeId = employeeId,
                                OrderDate = reader[3] as DateTime?,
                                RequiredDate = reader[4] as DateTime?,
                                ShippedDate = reader[5] as DateTime?,
                                ShipVia = shipVia,
                                Freight = freight,
                                ShipName = reader[8].ToString(),
                                ShipAddress = reader[9].ToString(),
                                ShipCity = reader[10].ToString(),
                                ShipRegion = reader[11].ToString(),
                                ShipPostalCode = reader[12].ToString(),
                                ShipCountry = reader[13].ToString()
                            };
                        }

                        reader.Close();
                    }
                }

                if (order.Details == null)
                {
                    order.Details = new List<OrdersDetail>();
                }

                order.Details.AddRange(GetOrderDetails(orderId));

                return order;
            });
        }

        public bool CreateNewOrder(Entities.Order order)
        {
            return GetConnection(con =>
            {
                int rowAffected;
                using (var command = CreateCommand<SqlCommand>(con, CreateNewOrderQuery))
                {
                    command.Parameters.AddWithValue("@customerId", order.CustomerId);
                    command.Parameters.AddWithValue("@employeeId", order.EmployeeId);
                    command.Parameters.AddWithValue("@orderDate", order.OrderDate);
                    command.Parameters.AddWithValue("@requiredDate", order.RequiredDate);
                    command.Parameters.AddWithValue("@shippedDate", order.ShippedDate);
                    command.Parameters.AddWithValue("@shipVia", order.ShipVia);
                    command.Parameters.AddWithValue("@freight", order.Freight);
                    command.Parameters.AddWithValue("@shipName", order.ShipName);
                    command.Parameters.AddWithValue("@shipAddress", order.ShipAddress);
                    command.Parameters.AddWithValue("@shipCity", order.ShipCity);
                    command.Parameters.AddWithValue("@shipRegion", order.ShipRegion);
                    command.Parameters.AddWithValue("@shipPostalCode", order.ShipPostalCode);
                    command.Parameters.AddWithValue("@shipCountry", order.ShipCountry);
                    rowAffected = command.ExecuteNonQuery();
                }

                return rowAffected == 1;
            });
        }

        public Order GetOrderById(int orderId)
        {
            return GetConnection(con =>
            {
                Order ord = null;
                using (var command = CreateCommand<SqlCommand>(con, GetOrderByIdQuery))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[0] is int ordId) || !(reader[2] is int employeeId) ||
                                !(reader[6] is int shipVia) || !(reader[7] is decimal freight))
                            {
                                continue;
                            }

                            ord = new Order
                            {
                                OrderId = ordId,
                                CustomerId = reader[1].ToString(),
                                EmployeeId = employeeId,
                                OrderDate = reader[3] as DateTime?,
                                RequiredDate = reader[4] as DateTime?,
                                ShippedDate = reader[5] as DateTime?,
                                ShipVia = shipVia,
                                Freight = freight,
                                ShipName = reader[8].ToString(),
                                ShipAddress = reader[9].ToString(),
                                ShipCity = reader[10].ToString(),
                                ShipRegion = reader[11].ToString(),
                                ShipPostalCode = reader[12].ToString(),
                                ShipCountry = reader[13].ToString()
                            };

                            if (ord.OrderDate == null)
                            {
                                ord.OrderState = OrderState.New;
                            }
                            else if (ord.ShippedDate == null)
                            {
                                ord.OrderState = OrderState.InWork;
                            }
                            else
                            {
                                ord.OrderState = OrderState.Done;
                            }
                        }

                        reader.Close();
                    }

                    return ord;
                }
            });
        }

        public bool UpdateOrder(Entities.Order order)
        {
            return GetConnection(con =>
            {
                int rowAffected;
                if (order.OrderId == null)
                {
                    return false;
                }

                var ord = GetOrderById((int) order.OrderId);
                if (ord == null || ord.OrderState != OrderState.New)
                {
                    return false;
                }

                using (var command = CreateCommand<SqlCommand>(con, UpdateOrderQuery))
                {
                    command.Parameters.AddWithValue("@orderId", order.OrderId);
                    command.Parameters.AddWithValue("@customerId", order.CustomerId);
                    command.Parameters.AddWithValue("@employeeId", order.EmployeeId);
                    command.Parameters.AddWithValue("@requiredDate", order.RequiredDate);
                    command.Parameters.AddWithValue("@shipVia", order.ShipVia);
                    command.Parameters.AddWithValue("@freight", order.Freight);
                    command.Parameters.AddWithValue("@shipName", order.ShipName);
                    command.Parameters.AddWithValue("@shipAddress", order.ShipAddress);
                    command.Parameters.AddWithValue("@shipCity", order.ShipCity);
                    command.Parameters.AddWithValue("@shipRegion", order.ShipRegion);
                    command.Parameters.AddWithValue("@shipPostalCode", order.ShipPostalCode);
                    command.Parameters.AddWithValue("@shipCountry", order.ShipCountry);
                    rowAffected = command.ExecuteNonQuery();
                }

                return rowAffected == 1;
            });
        }

        public bool DeleteNewAndInWorkOrders()
        {
            return GetConnection(con =>
            {
                int rowsAffected;
                using (var command = CreateCommand<SqlCommand>(con, DeleteNewAndInWorkOrdersQuery))
                {
                    rowsAffected = command.ExecuteNonQuery();
                }

                return rowsAffected > 0;
            });
        }

        public bool TransferOrderToWork(int orderId, DateTime orderDate)
        {
            var order = GetOrderById(orderId);
            if (order.OrderDate != null)
            {
                return false;
            }
            return GetConnection(con =>
            {
                int rowAffected;
                using (var command = CreateCommand<SqlCommand>(con, UpdateOrderDateQuery))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    command.Parameters.AddWithValue("@orderDate", orderDate);
                    rowAffected = command.ExecuteNonQuery();
                }
                return rowAffected == 1;
            });
        }

        public bool MarkOrderAsDone(int orderId, DateTime shippedDate)
        {
            var order = GetOrderById(orderId);
            if (order.ShippedDate != null)
            {
                return false;
            }
            return GetConnection(con =>
            {
                int rowAffected;
                using (var command = CreateCommand<SqlCommand>(con, UpdateShippedDateQuery))
                {
                    command.Parameters.AddWithValue("@orderId", orderId);
                    command.Parameters.AddWithValue("@shippedDate", shippedDate);
                    rowAffected = command.ExecuteNonQuery();
                }
                return rowAffected == 1;
            });
        }

        public List<ProductInfo> GetHistoryOfCustomerById(string customerId)
        {
            return GetConnection(con =>
            {
                var res = new List<ProductInfo>();
                using (var command = CreateCommand<SqlCommand>(con, CustOrderHistStoredProcedure, commandType:CommandType.StoredProcedure))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[1] is int qty))
                            {
                                throw new ArgumentException($"SqlTypes doesn't match with CLR types.\nStoredProcedure: {CustOrderHistStoredProcedure}");
                            }
                            res.Add(new ProductInfo
                            {
                                Title = reader[0].ToString(),
                                Qty = qty
                            });
                        }
                        reader.Close();
                    }

                    return res;
                }
            });
        }

        public List<OrdersDetail> GetOrderDetails(int orderId)
        {
            return GetConnection(con =>
            {
                var details = new List<OrdersDetail>();
                using (var command = CreateCommand<SqlCommand>(con, CustOrdersDetailProcedure, commandType: CommandType.StoredProcedure))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[1] is decimal unitPrice) || !(reader[2] is short quantity) || !(reader[3] is int discount) || !(reader[4] is decimal extendedPrice))

                            {
                                throw new ArgumentException($"SqlTypes doesn't match with CLR types.\nStoredProcedure: {CustOrdersDetailProcedure}");
                            }

                            details.Add(new OrdersDetail
                            {
                                ProductName = reader[0].ToString(),
                                UnitPrice = unitPrice,
                                Quantity = quantity,
                                Discount = discount,
                                ExtendedPrice = extendedPrice
                            });
                        }
                        reader.Close();
                    }
                }

                return details;
            });
        }
    }
}
