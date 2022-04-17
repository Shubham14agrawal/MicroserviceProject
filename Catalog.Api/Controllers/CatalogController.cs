using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        /// <summary>
        /// Service List
        /// </summary>
        private static readonly string[] ServiceList = new[]
        {
            "Electricians", "Yoga Trainers", "Interior Designers"
        };

        /// <summary>
        ///  Logger Instance
        /// </summary>
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(ILogger<CatalogController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///  Service List
        /// </summary>
        [HttpGet]
        public List<string> Get()
        {
            return ServiceList.ToList();
        }
    }
}
