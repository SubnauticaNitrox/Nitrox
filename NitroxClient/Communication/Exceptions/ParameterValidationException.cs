using System;

namespace NitroxClient.Communication.Exceptions
{
    public class ParameterValidationException: Exception
    {
        public string FaultingParameterName { get; private set; }

        public ParameterValidationException(string faultingParameterName, string message) : base(message)
        {
            FaultingParameterName = faultingParameterName;
        }
    }
}