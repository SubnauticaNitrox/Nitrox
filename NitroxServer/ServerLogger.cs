using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NitroxServer
{
    class ServerLogger : TextWriter
    {
        public ServerLogger()
        {
            prevOut = Console.Out;
            Console.SetOut(this);
            
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.ASCII;
            }
        }

        StreamWriter stream = new StreamWriter(new FileStream("output_log.txt", FileMode.OpenOrCreate));
        TextWriter prevOut;

        public override void WriteLine(string str)
        {
            stream.WriteLine(str);
            stream.Flush();
            Console.SetOut(prevOut);
            Console.WriteLine(str);
            Console.SetOut(this);
        }
    }
}
