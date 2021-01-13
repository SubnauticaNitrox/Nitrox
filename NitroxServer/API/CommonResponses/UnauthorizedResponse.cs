using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxPublic.API.CommonResponses
{
    public class UnauthorizedResponse
    {
        public string Message;
        public UnauthorizedResponse(string Message) { this.Message = Message; }
    }
}
