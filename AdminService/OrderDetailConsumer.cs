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
    public class OrderDetailConsumer : IConsumer<OrderDetail>
    {
        /// <summary>
        ///  Updated Order Details
        /// </summary>
        public static List<OrderDetail> updatedOrderdetails;

        public OrderDetailConsumer(IOptions<List<Common.OrderDetail>> orderDetailList)
        {
            updatedOrderdetails = orderDetailList.Value;
        }

        /// <summary>
        /// Consume RabbitMq Messages 
        /// </summary>
        public async Task Consume(ConsumeContext<OrderDetail> context)
        {
            var receivedmessage = context.Message;
            if (receivedmessage != null && updatedOrderdetails !=null)
            {
                var existingRecord = updatedOrderdetails.FirstOrDefault(x => x.OrderId.Equals(receivedmessage.OrderId));
                if (existingRecord != null)
                {
                    updatedOrderdetails.Remove(existingRecord);
                }

                updatedOrderdetails.Add(receivedmessage);
            }
        }
    }
}
