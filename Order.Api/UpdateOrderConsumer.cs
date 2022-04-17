using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Order.Api.Controllers;

namespace Order.Api
{
    [Serializable]
    public class UpdateOrderConsumer : IConsumer<Common.Order>
    {
        /// <summary>
        ///  current status of orders 
        /// </summary>
        public List<Common.Order> currentStatus = null;

        /// <summary>
        /// IbusControl Instance
        /// </summary>
        private readonly IBusControl _bus;

        /// <summary>
        /// Iconfiguration Instance
        /// </summary>
        private readonly IConfiguration _config;

        public UpdateOrderConsumer(IBusControl bus, IConfiguration config, IOptions<List<Common.Order>> orderlist)
        {
            this.currentStatus = orderlist.Value;
            _bus = bus;
            _config = config;
        }

        /// <summary>
        /// Consume the rabbitmq message
        /// </summary>
        public async Task Consume(ConsumeContext<Common.Order> context)
        {
            var receivedmessage = context.Message;
            var updateStatus = this.currentStatus.Where(x => x.OrderId.Equals(receivedmessage.OrderId)).FirstOrDefault();
            if (updateStatus != null) 
            {
                updateStatus.Status = receivedmessage.Status;
                if (updateStatus.Status.Equals("Accepted"))
                {
                    updateStatus.operatorDetails = receivedmessage.operatorDetails;
                }
                else 
                {
                    updateStatus.operatorDetails = null;
                }
            }
        }
    }
}
