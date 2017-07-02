using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            { 
                Listener listener = new Listener();
                listener.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
