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
        private const string CreateNewOrderQuery = "Insert into Orders (CustomerID, EmployeeID, RequiredDate) Values ";

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

                using (var command = CreateCommand<SqlCommand>(con, GetOrderProcedure, commandType:CommandType.StoredProcedure))
                {
                    CreateSqlParameter(command, "@OrderId", SqlDbType.Int, direction: ParameterDirection.Input, value: orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (!(reader[0] is int ordId) || !(reader[2] is int employeeId) || !(reader[6] is int shipVia) || !(reader[7] is decimal freight))
                            {
                                throw new ArgumentException($"SqlTypes doesn't match with CLR types.\nStoredProcedure: {GetOrderProcedure}");
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

                using (var command = CreateCommand<SqlCommand>(con, CustOrdersDetailProcedure, commandType:CommandType.StoredProcedure))
                {
                    CreateSqlParameter(command, "@OrderId", SqlDbType.Int, direction: ParameterDirection.Input, value: orderId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!(reader[1] is decimal unitPrice) || !(reader[2] is short quantity) || !(reader[3] is int discount) || !(reader[4] is decimal extendedPrice))

                            {
                                throw new ArgumentException($"SqlTypes doesn't match with CLR types.\nStoredProcedure: {CustOrdersDetailProcedure}");
                            }

                            order.Details.Add(new OrdersDetail
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

                return order;
            });
        }

        public bool CreateNewOrder(string customerId, int employeeId, DateTime requiredDate)
        {
            return GetConnection(con =>
            {
                var rowAffected = 0;
                var sb = new StringBuilder(CreateNewOrderQuery);
                sb.Append('(');
                sb.Append(customerId);
                sb.Append(',');
                sb.Append(employeeId);
                sb.Append(',');
                sb.Append(requiredDate);
                sb.Append(')');

                using (var command = CreateCommand<SqlCommand>(con, sb.ToString()))
                {
                    rowAffected = command.ExecuteNonQuery();
                }

                return rowAffected == 1;
            });
        }


    }
}
