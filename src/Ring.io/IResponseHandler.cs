using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ring.io.Messages;

namespace Ring.io
{
    public interface IResponseHandler
    {
        void HandleResponse(Message response);
    }
}
