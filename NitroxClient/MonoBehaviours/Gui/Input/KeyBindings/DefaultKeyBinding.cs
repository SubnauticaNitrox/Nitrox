namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class DefaultKeyBinding
    {
        public string binding { get; set; }
        public GameInput.BindingSet bindingSet { get; set; }

        public DefaultKeyBinding(string defaultBinding, GameInput.BindingSet defaultBindingSet)
        {
            binding = defaultBinding;
            bindingSet = defaultBindingSet;
        }
    }
}
