using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;

namespace LuxonLauncher
{
    public static class NetworkHelper
    {
        public static List<NetworkAdapterInfo> GetIPv4Adapters()
        {
            var adapters = new List<NetworkAdapterInfo>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                var ipv4 = ipProps.UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ipv4 != null)
                {
                    adapters.Add(new NetworkAdapterInfo
                    {
                        Name = ni.Name,
                        IPAddress = ipv4.Address.ToString()
                    });
                }
            }

            return adapters;
        }
    }
}