using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.Helper;

[TestClass]
public class KeyValueStoreTest
{
    [TestMethod]
    public void SetAndReadValue()
    {
        const string TEST_KEY = "test";

        KeyValueStore.Instance.SetValue(TEST_KEY, -50);
        Assert.AreEqual(-50, KeyValueStore.Instance.GetValue<int>(TEST_KEY));

        KeyValueStore.Instance.SetValue(TEST_KEY, 1337);
        Assert.AreEqual(1337, KeyValueStore.Instance.GetValue<int>(TEST_KEY));

        // Cleanup
        KeyValueStore.Instance.DeleteKey(TEST_KEY);
        Assert.IsNull(KeyValueStore.Instance.GetValue<int?>(TEST_KEY));
        Assert.IsFalse(KeyValueStore.Instance.KeyExists(TEST_KEY));
    }
}
