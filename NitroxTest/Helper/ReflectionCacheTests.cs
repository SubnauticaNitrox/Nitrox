using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.Helper;

namespace NitroxTest.Helper
{
    [TestClass]
    public class ReflectionCacheTests
    {
        [TestMethod]
        public void CacheIntReturnDelegate()
        {
            NitroxModel.DataStructures.Int3 num = new NitroxModel.DataStructures.Int3(0, 1, 2);
            Func<NitroxModel.DataStructures.Int3, int> method = ReflectionCache.InstanceMethod<NitroxModel.DataStructures.Int3, int>("get_Y", num);
            method.Should().NotBeNull();
            method.Invoke(num).ShouldBeEquivalentTo(1);
        }
    }
}
