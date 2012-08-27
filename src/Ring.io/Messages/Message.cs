using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ring.io.Messages
{
    public class Message
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public DateTime DateTime { get; set; }

        private IMessage msg;
        public IMessage Msg
        {
            get { return msg; }
            set
            {
                this.msg = value;
                if (this.msg != null)
                {
                    this.Type = this.msg.GetType().Name;
                }
            }
        }
    }
}
