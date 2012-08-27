using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Ring.io
{
    public class HashTableEntry
    {
        public byte[] NodeId { get; set; }
        public byte[] RingToken { get; set; }
        public IPEndPoint Address { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
