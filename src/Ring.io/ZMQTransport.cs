using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ring.io.Messages;
using ServiceStack.Text;
using ZMQ;

namespace Ring.io
{
    public class ZMQTransport
    {
        private Context context;
        private Socket socket;
        private JsonSerializer<Message> serializer;

        public ZMQTransport(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
            this.serializer = new JsonSerializer<Message>();
            this.Handlers = new List<RequestHandler>();
        }

        public IPEndPoint EndPoint { get; private set; }
        public List<RequestHandler> Handlers { get; set; }

        public void Open()
        {
            // Initialize ZeroMQ socket that will receive heartbeats.
            this.context = new Context();
            this.socket = this.context.Socket(SocketType.REP);
            this.socket.Bind(string.Format("tcp://{0}:{1}", this.EndPoint.Address, this.EndPoint.Port));

            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var request = this.socket.Recv(Encoding.Unicode);
                        this.HandleRequest(request, this.socket);
                    }
                });
        }

        public void Close()
        {
            this.socket.Dispose();
            this.context.Dispose();
        }

        public void Send(string text)
        {
            this.socket.Send(text, Encoding.Unicode);
        }

        public void Send(string text, IPEndPoint endPoint)
        {
            Task.Factory.StartNew(() =>
                {
                    using (var ctx = new Context())
                    using (var requestSocket = ctx.Socket(SocketType.REQ))
                    {
                        string addressString = "tcp://" + endPoint;
                        requestSocket.Connect(addressString);
                        requestSocket.Send(text, Encoding.Unicode);
                        var response = requestSocket.Recv(Encoding.Unicode);
                        HandleRequest(response, requestSocket);
                    }
                });
        }

        private void HandleRequest(string requestText, Socket socket)
        {
            if (requestText != string.Empty)
            {
                var request = this.serializer.DeserializeFromString(requestText);
                var response = new Message();
                response.CorrelationId = request.Id;
                response.Destination = request.Source;
                foreach (var handler in this.Handlers)
                {
                    handler(request, response);
                }
                if (request.CorrelationId != null)
                {
                    socket.Send(this.serializer.SerializeToString(response), Encoding.Unicode);
                }
                else
                {
                    socket.Send();
                }
            }
        }
    }
}