using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Models
{
    public class ConversationData
    {
        public bool PromptedUserForName { get; set; } = false;
        public TicketData TicketData { get; set; } 

    }

    public class TicketData
    {
        public string UserName { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public string RequestType { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
    }
}
