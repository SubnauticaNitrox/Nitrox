using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroxServer
{
    public class ConsoleWriter : TextWriter
    {
        public override void Write(char value)
        {
            MainWindow.Instance.WriteLog(value);
        }
        public override void WriteLine(string value)
        {
            MainWindow.Instance.WriteLog(value);
        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }
    }
}
