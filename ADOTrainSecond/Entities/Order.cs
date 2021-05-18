using System;

namespace ADOTrainSecond.Entities
{
    public class Order
    {
        public int? OrderId { get; set; } = null;
        public string CustomerId { get; set; } = null;
        public int? EmployeeId { get; set; } = null;
        public DateTime? OrderDate { get; set; } = null;
        public DateTime? RequiredDate { get; set; } = null;
        public DateTime? ShippedDate { get; set; } = null;
        public int? ShipVia { get; set; } = null;
        public decimal? Freight { get; set; } = null;
        public string ShipName { get; set; } = null;
        public string ShipAddress { get; set; } = null;
        public string ShipCity { get; set; } = null;
        public string ShipRegion { get; set; } = null;
        public string ShipPostalCode { get; set; } = null;
        public string ShipCountry { get; set; } = null;

        public static implicit operator Order(Dtos.Order orderDto)
        {
            return new Order
            {
                OrderId = orderDto.OrderId,
                CustomerId = orderDto.CustomerId,
                EmployeeId = orderDto.EmployeeId,
                OrderDate = orderDto.OrderDate,
                RequiredDate = orderDto.RequiredDate,
                ShippedDate = orderDto.ShippedDate,
                ShipVia = orderDto.ShipVia,
                Freight = orderDto.Freight,
                ShipName = orderDto.ShipName,
                ShipAddress = orderDto.ShipAddress,
                ShipCity = orderDto.ShipCity,
                ShipRegion = orderDto.ShipRegion,
                ShipPostalCode = orderDto.ShipPostalCode,
                ShipCountry = orderDto.ShipCountry
            };
        }
    }
}
