using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        /// <summary>
        ///  service list
        /// </summary>
        private Dictionary<int, string> serviceList = new Dictionary<int, string>()
            {
                { 100, "Electritions" },
                { 102,"Yoga Trainers"},
                { 103,"Interior Designers"}
            };

        /// <summary>
        ///  current status of orders 
        /// </summary>
        public List<Common.Order> currentStatus = null;

        /// <summary>
        /// IBusControl Instance
        /// </summary>
        private readonly IBusControl _bus;

        /// <summary>
        /// Iconfiguration Instance
        /// </summary>
        private readonly IConfiguration _config;
        public OrderController(IBusControl bus, IConfiguration config,IOptions<List<Common.Order>> orderlist)
        {
            this.currentStatus = orderlist.Value;
            _bus = bus;
            _config = config;
        }

        /// <summary>
        /// Order details 
        /// </summary>
        /// <returns> Order Details</returns>
        [HttpPost("/order")]
        public async Task<Common.Order> OrderAsync(User details)
        {
            var orderDetails = details.OrderDetails.FirstOrDefault();
            orderDetails.OrderId = this.generateOrderId();
            orderDetails.ServiceName = this.getServiceNames(orderDetails.ServiceId);
            orderDetails.Status = "Awaiting Approval";
            Uri uri = new Uri($"rabbitmq://{_config.GetValue<string>("RabbitMQHostName")}/orderDetails");
            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(orderDetails);

            if (!this.currentStatus.Where(x => x.OrderId.Equals(orderDetails.OrderId)).Any())
            {
                this.currentStatus.Add(orderDetails);
            }

            return orderDetails;
        }

        /// <summary>
        /// Order status
        /// </summary>
        /// <returns> Order Status</returns>
        [HttpPost("/getStatus")]
        public Common.Order Getstatus(Common.Order order)
        {

            return this.currentStatus.Where(x => x.OrderId.Equals(order.OrderId)).FirstOrDefault();
        }

        /// <summary>
        /// Generate Unique Order Id
        /// </summary>
        /// <returns> Unique Oder Id</returns>
        public string generateOrderId()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
               .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            string id = builder.ToString();
            return id;
        }

        /// <summary>
        /// Services Name
        /// </summary>
        /// <returns> Services Name</returns>
        public string  getServiceNames(int serviceId)
        {
          
            return this.serviceList.FirstOrDefault(x => x.Key == serviceId).Value;
        }
    }
}
