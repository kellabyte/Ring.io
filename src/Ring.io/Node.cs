using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ring.io.Messages;

namespace Ring.io
{
    public class Node
    {
        private const int DEFAULT_PORT = 5991;
        private const string DEFAULT_IPADDRESS = "127.0.0.1";
        private ZMQTransport transport;
        private Timer heartBeatTimer;
        private MD5 hash;
        private readonly Random random = new Random();
        private MessageBus messageBus;

        public Node()
            : this(DEFAULT_IPADDRESS, DEFAULT_PORT)
        {
        }

        public Node(string ipAddress)
            : this(ipAddress, DEFAULT_PORT)
        {

        }

        public Node(int port)
            : this(DEFAULT_IPADDRESS, port)
        {
        }

        public Node(string ipAddress, int port)
            : this(new IPEndPoint(IPAddress.Parse(ipAddress), port))
        {
        }

        public Node(IPEndPoint endpoint)
        {
            this.Nodes = new Dictionary<string, HashTableEntry>();
            this.Entry = new HashTableEntry();
            this.Entry.Address = endpoint.ToString();

            hash = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(endpoint.ToString());
            this.Entry.NodeId = new BigInteger(hash.ComputeHash(bytes)).ToString();
            this.Entry.LastSeen = DateTime.MinValue;
        }

        public HashTableEntry Entry { get; private set; }
        public Dictionary<string, HashTableEntry> Nodes { get; private set; }

        public void Open()
        {
            if (transport != null)
            {
                throw new InvalidOperationException("Node already active");
            }

            // Initialize the communication transport.
            this.transport = new ZMQTransport(IPEndPointParser.Parse(this.Entry.Address));
            this.transport.Open();
            this.messageBus = new MessageBus(this, this.transport);

            // Initialize the heartbeat timer.
            heartBeatTimer = new Timer(HeartBeatTimer, null, 1000, 1000);
        }

        public void Close()
        {
            heartBeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this.transport.Close();
        }

        // TODO: This should take only seed IP:Port since thats all the config will have
        public void AddSeedNode(HashTableEntry seedEntry)
        {
            if (this.Entry.NodeId == seedEntry.NodeId || this.Nodes.ContainsKey(seedEntry.NodeId))
            {
                return;
            }
            this.Nodes.Add(seedEntry.NodeId, seedEntry);
        }

        private void HeartBeatTimer(object state)
        {
            var heartbeat = new HeartBeat();
            heartbeat.Nodes = this.Nodes;

            var msg = new Message();
            messageBus.AddMessage<HeartBeat>(msg, heartbeat);

            // Choose a random node from the ring to gossip with.
            var nodeNumber = random.Next(0, this.Nodes.Count);
            if (this.Nodes.Count > 0)
            {
                var destinationEndpoint = IPEndPointParser.Parse(this.Nodes.ElementAt(nodeNumber).Value.Address);
                this.messageBus.Send(msg, destinationEndpoint);
            }
        }
    }
}
