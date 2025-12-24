#if NET9_0_OR_GREATER
using System;

namespace Nitrox.Model.Extensions;

public static class RangeExtensions
{
    public static bool IsEmpty(this Range range) => range.End.Value - range.Start.Value <= 0;
}
#endif
