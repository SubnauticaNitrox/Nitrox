namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class DefaultKeyBinding
    {
        public string Binding { get; private set; }
        public GameInput.BindingSet BindingSet { get; private set; }

        public DefaultKeyBinding(string defaultBinding, GameInput.BindingSet defaultBindingSet)
        {
            Binding = defaultBinding;
            BindingSet = defaultBindingSet;
        }
    }
}
