using System;
using System.Runtime.InteropServices;
using ADOTrainSecond.Dtos;
using ADOTrainSecond.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SqlRepositoryTests
    {
        [TestMethod]
        public void GetOrdersTest()
        {
            var repo = new SqlRepository();
            var res = repo.GetOrders();
            foreach (var order in res)
            {
                Assert.IsFalse(order.OrderState == OrderState.New && order.OrderDate != null);
                Assert.IsFalse(order.OrderState == OrderState.InWork && order.ShippedDate != null);
                Assert.IsFalse(order.OrderState == OrderState.Done && order.ShippedDate == null);
            }
        }

        [TestMethod]
        public void GetDetailedOrderTest()
        {
            const int orderId = 10248;
            var repo = new SqlRepository();
            var res = repo.GetDetailedOrder(orderId);
            Assert.IsTrue(res.Details.Count > 0);
            Assert.IsTrue(res.OrderId == orderId);
        }

        [TestMethod]
        public void CreateNewOrderTest()
        {
            const string customerId = "VINET";
            const int empolyeeId = 5;
            var requiredDate = DateTime.Now.AddDays(7);
            var repo = new SqlRepository();
            Assert.IsTrue(repo.CreateNewOrder(customerId, empolyeeId, requiredDate));
        }
    }
}
