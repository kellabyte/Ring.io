using System;
using System.Net;
using System.Threading.Tasks;
using Ring.io.Messages;
using ServiceStack.Text;

namespace Ring.io
{
    public class MessageBus : IRequestHandler, IResponseHandler
    {
        private Node node;
        private ZMQTransport transport;
        private JsonSerializer<Message> serializer;

        public MessageBus(Node node, ZMQTransport transport)
        {
            this.node = node;
            this.transport = transport;
            this.serializer = new JsonSerializer<Message>();

            this.transport.RequestHandlers.Add(this);
            this.transport.ResponseHandlers.Add(this);
        }

        public void HandleRequest(Message request, Message response)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format(
            //    "{0}\t{1} received {2}",
            //    DateTime.Now,
            //    this.node.Entry.Address,
            //    sourceEndPoint.Port));

            // TODO: Here we handle the messages that get received by the node.
            // TODO: Call methods on the Node class to merge hash rings and do failure detection.
            // TODO: Liskov substitution principle violation that I'm not happy about.

            if (response != null)
            {
                Process(request);

                response.SourceAddress = this.node.Entry.Address;
                response.SourceNodeId = this.node.Entry.NodeId;

                var heartbeat = new HeartBeat();
                heartbeat.Nodes = node.Nodes;

                this.AddMessage<HeartBeat>(response, heartbeat);

                string[] sourceAddress = request.SourceAddress.Split(':');
                var sourceEndPoint = new IPEndPoint(IPAddress.Parse(sourceAddress[0]), int.Parse(sourceAddress[1]));
            }
        }

        public void HandleResponse(Message response)
        {
            // TODO: Here we handle the messages that get received by the node.
            // TODO: Call methods on the Node class to merge hash rings and do failure detection.
            // TODO: Liskov substitution principle violation that I'm not happy about.

            //System.Diagnostics.Debug.WriteLine(string.Format(
            //    "{0}\t{1} received {2}",
            //    DateTime.Now,
            //    this.node.Entry.Address,
            //    sourceEndPoint.Port));

            Process(response);
        }

        public void Send(Message message, IPEndPoint endPoint)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format(
            //    "{0} SENT {1}",
            //    this.node.Entry.Address,
            //    message.Id));

            message.SourceAddress = this.node.Entry.Address;
            message.SourceNodeId = this.node.Entry.NodeId;
            message.DestinationAddress = endPoint.ToString();
            string msg = this.serializer.SerializeToString(message);
            this.transport.Send(msg, endPoint);
        }

        public void AddMessage<T>(Message message, T msg)
        {
            string serializedMessage = JsonSerializer.SerializeToString<T>(msg);
            message.Messages.Add(msg.GetType().Name.ToLowerInvariant(), serializedMessage);
        }

        private void Process(Message message)
        {
            string msg;
            if (message.Messages.TryGetValue(typeof(HeartBeat).Name.ToLower(), out msg))
            {
                var receivedHeartBeat = JsonSerializer.DeserializeFromString<HeartBeat>(msg);
                this.node.MergeTables(message.SourceNodeId, receivedHeartBeat.Nodes);
            }
        }

        private void UpdateLastUpdated(Message message)
        {
            // TODO
        }
    }
}