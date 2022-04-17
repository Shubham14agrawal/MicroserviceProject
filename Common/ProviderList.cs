using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Common
{
    public class ProviderList
    {
        public List<ServiceProvider> providerList = new List<ServiceProvider>();

        public ProviderList() 
        {
            this.AddProviders(100, "Electricians ");
            this.AddProviders(101, "Yoga Trainers ");
            this.AddProviders(102, "Interior Designers ");
        }
        public void AddProviders(int serviceId, string serviceName)
        {
            for (int i = 1;  i < 4; i++)
            {
                var provider = new ServiceProvider();
                provider.Avalibility = i % 2 == 0;
                provider.ProviderName = serviceName + i;
                provider.ProviderId = serviceName.Substring(0, 3) + i;
                provider.ServiceId = serviceId;
                this.providerList.Add(provider);
            }
        }
    }
}
