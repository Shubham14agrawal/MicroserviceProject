using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminServiceController : ControllerBase
    {
        /// <summary>
        ///  IBusControl Instance
        /// </summary>
        private readonly IBusControl _bus;

        /// <summary>
        ///  IConfiguration Instance
        /// </summary>
        private readonly IConfiguration _config;

        public AdminServiceController(IBusControl bus, IConfiguration config)
        {
            _bus = bus;
            _config = config;
        }

        /// <summary>
        ///  Update Status of Orders 
        /// </summary>
        [HttpPost("/updateStatus")]
        public async Task updateStatusAsync(Common.Order order) 
        {
            var updatedStatus = OrderConsumer.receivedOrder.Where( x => x.OrderId.Equals(order.OrderId)).FirstOrDefault();
            var orderDetails = OrderDetailConsumer.updatedOrderdetails;
            if (updatedStatus != null && !(orderDetails != null ))
            {
                updatedStatus.Status = "Finding Service Provider";
            }
            else if (orderDetails.Any())
            {
                var existingRecord = orderDetails.Where(x => x.OrderId.Equals(order.OrderId)).FirstOrDefault();
                if (existingRecord !=null && existingRecord.OrderId.Equals(order.OrderId))
                {
                    updatedStatus.operatorDetails = orderDetails.Where(x => x.OrderId.Equals(order.OrderId)).FirstOrDefault();
                    updatedStatus.Status = orderDetails.Where(x => x.OrderId.Equals(order.OrderId)).FirstOrDefault().Status;
                }
                else 
                {
                    updatedStatus.Status = "Finding Service Provider";
                }
            }

            Uri uri = new Uri($"rabbitmq://{_config.GetValue<string>("RabbitMQHostName")}/AdminUpdateStatus");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(updatedStatus);
        }

        /// <summary>
        ///  Available Operator List For Service
        /// </summary>
        [HttpGet("/GetProviderList")]
        public IEnumerable<Common.ServiceProvider> GetProviderList(Common.Order order)
        {
            var providerList = new Common.ProviderList().providerList.Where(x => x.Avalibility = true && x.ServiceId == order.ServiceId);
            return providerList;
        }

        /// <summary>
        ///  Send Order To Available Operator
        /// </summary>
        [HttpPost("/SendServiceRequest")]
        public async Task SendServiceRequestAsync(Common.Order order)
        {
            var availableProviders = new Common.ProviderList().providerList.Where(x => x.Avalibility = true && x.ServiceId == order.ServiceId).FirstOrDefault();
            var orderDetails = new Common.OrderDetail();
            orderDetails.OrderId = order.OrderId;
            orderDetails.ServiceId = order.ServiceId;
            orderDetails.ProviderId = availableProviders.ProviderId;
            orderDetails.ProviderName = availableProviders.ProviderName;

            Uri uri = new Uri($"rabbitmq://{_config.GetValue<string>("RabbitMQHostName")}/serviceProvider");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(orderDetails);
        }
    }
}
