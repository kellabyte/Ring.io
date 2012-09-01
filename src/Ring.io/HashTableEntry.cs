using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;

namespace Ring.io
{
    public class HashTableEntry
    {
        public string NodeId { get; set; }
        public string RingToken { get; set; }
        public string Address { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
