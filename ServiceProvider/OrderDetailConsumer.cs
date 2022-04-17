using MassTransit;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceProvider
{
    [Serializable]
    public class OrderDetailConsumer : IConsumer<Common.OrderDetail>
    {
        /// <summary>
        ///  Order Detail List 
        /// </summary>
        public static List<Common.OrderDetail> orderDetails;

        public OrderDetailConsumer(IOptions<List<Common.OrderDetail>> orderdetail)
        {
            orderDetails = orderdetail.Value;
        }

        /// <summary>
        ///  Consume RabbitMQ Messages
        /// </summary>
        public async Task Consume(ConsumeContext<Common.OrderDetail> context)
        {
            var receivedmessage = context.Message;
            var updateStatus = orderDetails.Where(x => x.OrderId.Equals(receivedmessage.OrderId)).FirstOrDefault();
            if (updateStatus != null)
            {
                updateStatus.Status = receivedmessage.Status;
            }
            else 
            {
                orderDetails.Add(receivedmessage);
            }
        }
    }
}
