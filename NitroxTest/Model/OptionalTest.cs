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
            Optional<string> op = Optional<string>.Of("test");
            Assert.AreEqual("test", op.Get());
        }

        [TestMethod]
        public void OptionalIsPresent()
        {
            Optional<string> op = Optional<string>.Of("test");
            Assert.AreEqual(true, op.IsPresent());
        }

        [TestMethod]
        public void OptionalIsNotPresent()
        {
            Optional<string> op = Optional<string>.Empty();
            Assert.AreEqual(false, op.IsPresent());
        }

        [TestMethod]
        public void OptionalOrElseValidValue()
        {
            Optional<string> op = Optional<string>.Of("test");
            Assert.AreEqual("test", op.OrElse("test2"));
        }

        [TestMethod]
        public void OptionalOrElseNoValue()
        {
            Optional<string> op = Optional<string>.Empty();
            Assert.AreEqual("test", op.OrElse("test"));
        }

        [TestMethod]
        public void OptionalEmpty()
        {
            Optional<string> op = Optional<string>.Empty();
            Assert.AreEqual(true, op.IsEmpty());
        }

        // Test functionality with value (non-nullable) types.

        [TestMethod]
        public void OptionalValueTypeGet()
        {
            Optional<int> op = Optional<int>.Of(1);
            Assert.AreEqual(1, op.Get());
        }

        [TestMethod]
        public void OptionalValueTypeIsPresent()
        {
            Optional<int> op = Optional<int>.Of(0);
            Assert.AreEqual(true, op.IsPresent());
        }

        [TestMethod]
        public void OptionalValueTypeIsNotPresent()
        {
            Optional<int> op = Optional<int>.Empty();
            Assert.AreEqual(false, op.IsPresent());
        }

        [TestMethod]
        public void OptionalValueTypeOrElseValidValue()
        {
            Optional<int> op = Optional<int>.Of(1);
            Assert.AreEqual(1, op.OrElse(2));
        }

        [TestMethod]
        public void OptionalSetValue()
        {
            Optional<int> op = Optional<int>.Of(0);
            op = 1;
            Assert.IsFalse(0 == (int)op);
        }

        [TestMethod]
        public void OptionalSetValueNull()
        {
            Optional<int> op = Optional<int>.Of(0);
            op = null;
            try
            {
                Assert.IsFalse(0 == (int)op);
            }
            catch (OptionalNullException<int>)
            {

            }
        }

        [TestMethod]
        public void OptionalValueTypeOrElseNoValue()
        {
            Optional<int> op = Optional<int>.Empty();
            Assert.AreEqual(1, op.OrElse(1));
        }

        [TestMethod]
        public void OptionalValueTypeEmpty()
        {
            Optional<int> op = Optional<int>.Empty();
            Assert.AreEqual(true, op.IsEmpty());
        }
    }
}
