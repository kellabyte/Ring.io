using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
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
        private Thread listenerThread;
        private bool opened = false;

        public ZMQTransport(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            this.EndPoint = endPoint;
            this.serializer = new JsonSerializer<Message>();
            this.RequestHandlers = new List<IRequestHandler>();
            this.ResponseHandlers = new List<IResponseHandler>();
        }

        public IPEndPoint EndPoint { get; private set; }
        public List<IRequestHandler> RequestHandlers { get; set; }
        public List<IResponseHandler> ResponseHandlers { get; set; }

        public void Open()
        {
            opened = true;

            // Initialize ZeroMQ socket that will receive heartbeats.
            this.context = new Context();
            this.socket = this.context.Socket(SocketType.REP);
            this.socket.Bind(string.Format("tcp://{0}:{1}", this.EndPoint.Address, this.EndPoint.Port));

            listenerThread = new Thread(new ThreadStart(HandleRequests));
            listenerThread.Start();
        }

        public void Close()
        {
            opened = false;
            this.socket.Dispose();
            this.context.Dispose();
        }

        public void Send(string message, IPEndPoint endPoint)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentNullException("message");
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    using (var ctx = new Context())
                    using (var requestSocket = ctx.Socket(SocketType.REQ))
                    {
                        string addressString = "tcp://" + endPoint;
                        requestSocket.Connect(addressString);
                        this.Send(requestSocket, message);
                        var response = requestSocket.Recv(Encoding.UTF8);
                        HandleMessage(response, requestSocket);
                    }
                }
                catch (System.Exception e)
                {
                    Tracer.Instance.WriteError(e);
                    if (ExceptionUtility.IsFatal(e))
                    {
                        throw;
                    }
                }
            });
        }

        private void HandleRequests()
        {
            while (opened)
            {
                try
                {
                    var bytes = this.socket.Recv();
                    var request = Encoding.UTF8.GetString(bytes);
                    this.HandleMessage(request, this.socket);
                }
                catch (System.Exception e)
                {
                    Tracer.Instance.WriteError(e);
                    if (ExceptionUtility.IsFatal(e))
                    {
                        throw;
                    }
                }
            }
        }

        private void HandleMessage(string message, Socket requestSocket)
        {
            if (message != string.Empty)
            {
                var request = this.serializer.DeserializeFromString(message);

                //System.Diagnostics.Debug.WriteLine(string.Format(
                //    "{0} RECV {1}",
                //    this.EndPoint.Port,
                //    request.Id));

                Message response = null;
                if (request.CorrelationId == null)
                {
                    response = new Message();
                    response.CorrelationId = request.Id;
                    response.DestinationAddress = request.SourceAddress;
                    foreach (var handler in this.RequestHandlers)
                    {
                        handler.HandleRequest(request, response);
                    }
                }
                else
                {
                    foreach (var handler in this.ResponseHandlers)
                    {
                        handler.HandleResponse(request);
                    }                    
                }
                
                if (request.CorrelationId == null)
                {
                    //System.Diagnostics.Debug.WriteLine(string.Format(
                    //    "{0} SENT {1}",
                    //    this.EndPoint.Port,
                    //    response.Id));

                    var msg = this.serializer.SerializeToString(response);
                    this.Send(requestSocket, msg);
                }
            }
        }

        private void Send(Socket socket, string message)
        {
            try
            {
                socket.Send(message, Encoding.UTF8);
            }
            catch (System.Exception e)
            {
                Tracer.Instance.WriteError(e);
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
            }
        }
    }
}