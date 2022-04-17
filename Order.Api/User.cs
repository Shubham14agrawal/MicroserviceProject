using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Api
{
    public class User
    {
        public User() {
            this.OrderDetails = new List<Common.Order>();
        }
        public int Userid { get; set; }

        public string Name { get; set; }

        public string Addresss { get; set; }

        public string Location { get; set; }

        public List<Common.Order> OrderDetails { get; set; }

    }
}
