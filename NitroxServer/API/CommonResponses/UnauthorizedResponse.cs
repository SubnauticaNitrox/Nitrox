using System;

namespace NitroxPublic.API.CommonResponses
{
    [Serializable]
    public class UnauthorizedResponse
    {
        public string Message;
        public UnauthorizedResponse(string Message) { this.Message = Message; }
    }
}
