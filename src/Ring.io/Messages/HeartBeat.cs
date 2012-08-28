using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Ring.io.Messages
{
    public class HeartBeat
    {
        public Dictionary<string, HashTableEntry> Nodes { get; set; }

        public HeartBeat()
        {
            this.Nodes = new Dictionary<string, HashTableEntry>();
        }
    }
}
