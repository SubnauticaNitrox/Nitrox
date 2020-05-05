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
        public void MakeBoxedInstanceDelegate()
        {
            B b = new B();
            object bObj = b;
            Func<object, string> method = ReflectionCache.InstanceMethod<string>("GetNameA", bObj);
            method(b).ShouldBeEquivalentTo("A from B");
            method(bObj).ShouldBeEquivalentTo("A from B");
        }

        [TestMethod]
        public void NullOnUnknownMethodName()
        {
            B b = new B();
            object bObj = b;
            Func<object, string> method = ReflectionCache.InstanceMethod<string>("GetNameC", bObj);
            method.Should().BeNull();
        }

        private class A
        {
            public virtual string GetNameA()
            {
                return "A";
            }
        }

        private class B : A
        {
            public override string GetNameA()
            {
                return $"{base.GetNameA()} from {nameof(B)}";
            }

            public string GetNameB()
            {
                return "B";
            }
        }
    }
}
