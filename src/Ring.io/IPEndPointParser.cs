using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Ring.io
{
    public class IPEndPointParser
    {
        public static IPEndPoint Parse(String endPoint)
        {
            string[] address = endPoint.Split(':');
            return new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1]));
        }
    }
}
