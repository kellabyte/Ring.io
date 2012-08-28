using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ring.io.Messages;

namespace Ring.io
{
    public delegate void RequestHandler(Message request, Message response);
}
