using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ring.io.Messages
{
    public class HeartBeat : IMessage
    {
        public Dictionary<byte[], HashTableEntry> Nodes { get; set; }

        public HeartBeat()
        {
            this.Nodes = new Dictionary<byte[], HashTableEntry>();
        }
    }
}
