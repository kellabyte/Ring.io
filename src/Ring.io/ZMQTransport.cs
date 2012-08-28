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
            this.RequestHandlers = new List<RequestHandler>();
            this.ResponseHandlers = new List<ResponseHandler>();
        }

        public IPEndPoint EndPoint { get; private set; }
        public List<RequestHandler> RequestHandlers { get; set; }
        public List<ResponseHandler> ResponseHandlers { get; set; }

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
                        var bytes = this.socket.Recv();
                        var request = Encoding.UTF8.GetString(bytes);
                        this.HandleMessage(request, this.socket);
                    }
                });
        }

        public void Close()
        {
            this.socket.Dispose();
            this.context.Dispose();
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
                        requestSocket.Send(text, Encoding.UTF8);
                        var response = requestSocket.Recv(Encoding.UTF8);
                        HandleMessage(response, requestSocket);
                    }
                });
        }

        private void HandleMessage(string message, Socket requestSocket)
        {
            if (message != string.Empty)
            {
                var request = this.serializer.DeserializeFromString(message);

                System.Diagnostics.Debug.WriteLine(string.Format(
                    "{0} RECV {1}",
                    this.EndPoint.Port,
                    request.Id));

                Message response = null;
                if (request.CorrelationId == null)
                {
                    response = new Message();
                    response.CorrelationId = request.Id;
                    response.Destination = request.Source;
                    foreach (var handler in this.RequestHandlers)
                    {
                        handler(request, response);
                    }
                }
                else
                {
                    foreach (var handler in this.ResponseHandlers)
                    {
                        handler(request);
                    }                    
                }
                
                if (request.CorrelationId == null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format(
                        "{0} SENT {1}",
                        this.EndPoint.Port,
                        response.Id));

                    var msg = this.serializer.SerializeToString(response);
                    requestSocket.Send(msg, Encoding.UTF8);
                }
            }
        }
    }
}