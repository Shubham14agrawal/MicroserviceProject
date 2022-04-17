using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceProvider.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ServicceProviderController : ControllerBase
    {
        /// <summary>
        ///  IBusControl Instance
        /// </summary>
        private readonly IBusControl _bus;

        /// <summary>
        ///  IConfiguration Instance  
        /// </summary>
        private readonly IConfiguration _config;
        public ServicceProviderController(IBusControl bus, IConfiguration config)
        {
            _bus = bus;
            _config = config;
        }

        /// <summary>
        ///  Sends Provider Response
        /// </summary>
        [HttpPost("/Provider/UpdateStatus")]
        public async Task SendtProviderResponseAsync(Common.OrderDetail orderDetail)
        {
            var orderdetails = OrderDetailConsumer.orderDetails;
            if (orderdetails != null)
            {
              var orderdetail = orderdetails.Where(x => x.ServiceId == orderDetail.ServiceId && x.OrderId.Equals(orderDetail.OrderId) && x.ProviderId.Equals(orderDetail.ProviderId)).FirstOrDefault();
                orderdetail.Status = orderDetail.Status;
                orderdetail.ExpectedArrival = orderDetail.ExpectedArrival;
                Uri uri = new Uri($"rabbitmq://{_config.GetValue<string>("RabbitMQHostName")}/ProviderUpdateStatus");
                var endPoint = await _bus.GetSendEndpoint(uri);
                await endPoint.Send(orderdetail);
            }
        }
    }
}
