using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxClient.Communication.Abstract
{
    public interface IConnectionInfo : IEquatable<IConnectionInfo>
    {
        string ToString();
    }
}
