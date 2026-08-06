using System.Linq;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

namespace Nitrox.Test.Client.Gui.Input;

[TestClass]
public sealed class KeyBindingManagerTest
{
    [TestMethod]
    public void VehicleHornBindingIsRegisteredForInputSettings()
    {
        VehicleHornKeyBindingAction binding = KeyBindingManager.KeyBindings
                                                              .OfType<VehicleHornKeyBindingAction>()
                                                              .Should()
                                                              .ContainSingle()
                                                              .Which;

        binding.ButtonLabel.Should().Be("Nitrox_Settings_Keybind_VehicleHorn");
        binding.DefaultKeyboardKey.Should().Be("h");
        binding.DefaultControllerKey.Should().Be("leftStickPress");
    }
}
