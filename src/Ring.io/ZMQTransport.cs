using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZMQ;

namespace Ring.io
{
    public class ZMQTransport
    {
        private Context context;
        private Socket socket;

        public ZMQTransport(IPEndPoint endPoint)
        {
            this.EndPoint = endPoint;
            this.Requests = new BlockingCollection<string>();
            this.Responses = new BlockingCollection<string>();
        }

        public IPEndPoint EndPoint { get; private set; }
        public BlockingCollection<string> Requests { get; private set; }
        public BlockingCollection<string> Responses { get; private set; }

        public void Open()
        {
            // Initialize ZeroMQ socket that will publish heartbeats.
            this.context = new Context();
            this.socket = this.context.Socket(SocketType.REP);
            this.socket.Bind(string.Format("tcp://{0}:{1}", this.EndPoint.Address, this.EndPoint.Port));

            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        var request = this.socket.Recv(Encoding.Unicode);
                        this.socket.Send();
                        this.Requests.Add(request);
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
                        string response = requestSocket.Recv(Encoding.Unicode);
                        if (response != string.Empty)
                        {
                            this.Responses.Add(response);
                        }
                    }
                });
        }
    }
}