using System;

namespace Common
{
    public class Order
    {
        public string OrderId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string Status { get; set; }

        public OrderDetail operatorDetails { get; set; }
    }
}
