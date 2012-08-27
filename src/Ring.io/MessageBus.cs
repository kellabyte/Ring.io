using System;
using System.Net;
using System.Threading.Tasks;
using Ring.io.Messages;
using ServiceStack.Text;

namespace Ring.io
{
    public class MessageBus
    {
        private Node node;
        private ZMQTransport transport;
        private JsonSerializer<Message> serializer;

        public MessageBus(Node node, ZMQTransport transport)
        {
            this.node = node;
            this.transport = transport;
            this.serializer = new JsonSerializer<Message>();

            this.HandleMessages();
        }

        private void HandleMessages()
        {
            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string message = this.transport.Requests.Take();
                        var msg = this.serializer.DeserializeFromString(message);
                        if (msg != null)
                        {
                            this.Handle(msg);
                        }
                    }
                });

            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        string message = this.transport.Responses.Take();
                        var msg = this.serializer.DeserializeFromString(message);
                        if (msg != null)
                        {
                            this.Handle(msg);
                        }
                    }
                });
        }

        private void Handle(Message message)
        {
            string[] sourceAddress = message.Source.Split(':');
            var sourceEndPoint = new IPEndPoint(IPAddress.Parse(sourceAddress[0]), int.Parse(sourceAddress[1]));

            System.Diagnostics.Debug.WriteLine(string.Format(
                "{0}\t{1} received {2}",
                DateTime.Now,
                this.node.Entry.Address.Port,
                sourceEndPoint.Port));

            // TODO: Here we handle the messages that get received by the node.
            // TODO: Call methods on the Node class to merge hash rings and do failure detection.
            // TODO: Liskov substitution principle violation that I'm not happy about.
            if (message is HeartBeat)
            {

            }
        }

        public void Send(Message message)
        {
            message.Source = this.transport.EndPoint.ToString();
            string msg = this.serializer.SerializeToString(message);
            this.transport.Send(msg);
        }

        public void Send(Message message, IPEndPoint endPoint)
        {
            message.Source = this.transport.EndPoint.ToString();
            string msg = this.serializer.SerializeToString(message);
            this.transport.Send(msg, endPoint);
        }
    }
}