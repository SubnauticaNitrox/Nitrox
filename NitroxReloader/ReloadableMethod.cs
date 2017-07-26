using System;

namespace NitroxReloader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ReloadableMethodAttribute : Attribute
    {
    }
}
