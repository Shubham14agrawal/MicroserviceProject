using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Common;
using Nancy.Json;
using Microsoft.Extensions.Options;

namespace AdminService
{
    [Serializable]
    public class OrderConsumer : IConsumer<Order>
    {
        /// <summary>
        ///  Received Order List
        /// </summary>
        public static List<Order> receivedOrder;
        public OrderConsumer(IOptions<List<Common.Order>> orderlist)
        {
            receivedOrder = orderlist.Value;
        }

        /// <summary>
        ///  Consume RabbitMq Messages 
        /// </summary>
        public async Task Consume(ConsumeContext<Order> context)
        {
            var receivedmessage = context.Message;
            if (receivedOrder.Where(x => x.OrderId.Equals(receivedmessage.OrderId)) !=null)
            {
                receivedOrder.Add(receivedmessage);
            }
        }
    }
}
