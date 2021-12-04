using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxModel.Networking
{
    public interface IConnectionInfo : IEquatable<IConnectionInfo>
    {
        string ToString();
    }
}
