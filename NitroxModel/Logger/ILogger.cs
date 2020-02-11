using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    public interface ILogger
    {
        void log(LogType type, string message);
    }
}
