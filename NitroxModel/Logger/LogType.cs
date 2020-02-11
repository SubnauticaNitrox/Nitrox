using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Logger
{
    /// <summary>
    /// https://github.com/nlog/nlog/wiki/Configuration-file#log-levels
    /// </summary>
    public enum LogType
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
    }
}
