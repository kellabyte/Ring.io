using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ring.io.Messages
{
    public class Message
    {
        public Message()
        {
            this.Id = Guid.NewGuid();
            this.DateTime = DateTime.UtcNow;
            this.Messages = new Dictionary<string, string>();
        }

        public Guid Id { get; set; }
        public Guid? CorrelationId { get; set; }
        public string SourceAddress { get; set; }
        public string SourceNodeId { get; set; }
        public string DestinationAddress { get; set; }
        public DateTime DateTime { get; set; }
        public Dictionary<string, string> Messages { get; set; }
    }
}
