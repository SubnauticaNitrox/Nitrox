using Nitrox.Server.Subnautica.Models.Helper;

namespace Nitrox.Test.Server.Helper;

[TestClass]
public sealed class EasyPoolTest
{
    [TestMethod]
    public void TestPoolRefInstanceNotSame()
    {
        using EasyPool<MyObj>.Lease lease1 = EasyPool<MyObj>.Rent();
        ref MyObj? value1 = ref lease1.GetRef();
        value1 = new MyObj(1);
        using EasyPool<MyObj>.Lease lease2 = EasyPool<MyObj>.Rent();
        ref MyObj? value2 = ref lease2.GetRef();
        value2 = new MyObj(2);

        lease1.GetRef().Should().NotBe(lease2.GetRef());
    }

    private record MyObj(int Number);
}
