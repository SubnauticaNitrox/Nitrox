using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxModel.DataStructures.Util;

namespace NitroxTest.Model
{
    [TestClass]
    public class OptionalTest
    {
        [TestMethod]
        public void OptionalGet()
        {
            Optional<String> op = Optional<String>.Of("test");
            Assert.AreEqual("test", op.Get());
        }

        [TestMethod]
        public void OptionalIsPresent()
        {
            Optional<String> op = Optional<String>.Of("test");
            Assert.AreEqual(true, op.IsPresent());
        }

        [TestMethod]
        public void OptionalIsNotPresent()
        {
            Optional<String> op = Optional<String>.Empty();
            Assert.AreEqual(false, op.IsPresent());
        }

        [TestMethod]
        public void OptionalOrElseValidValue()
        {
            Optional<String> op = Optional<String>.Of("test");
            Assert.AreEqual("test", op.OrElse("test2"));
        }

        [TestMethod]
        public void OptionalOrElseNoValue()
        {
            Optional<String> op = Optional<String>.Empty();
            Assert.AreEqual("test", op.OrElse("test"));
        }

        [TestMethod]
        public void OptionalEmpty()
        {
            Optional<String> op = Optional<String>.Empty();
            Assert.AreEqual(true, op.IsEmpty());
        }
    }
}
