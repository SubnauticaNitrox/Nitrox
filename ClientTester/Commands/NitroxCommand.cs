using ClientTester;
using NitroxClient.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientTester.Commands
{
    public abstract class NitroxCommand
    {
        public string Name;
        public string Description;
        public string Syntax;
        public abstract void Execute(MultiplayerClient client, string[] args);
    }
}
