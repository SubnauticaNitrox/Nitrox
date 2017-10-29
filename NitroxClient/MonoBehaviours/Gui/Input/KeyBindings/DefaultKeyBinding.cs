namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings
{
    public class DefaultKeyBinding
    {
        public string Binding { get; }
        public GameInput.BindingSet BindingSet { get; }

        public DefaultKeyBinding(string defaultBinding, GameInput.BindingSet defaultBindingSet)
        {
            Binding = defaultBinding;
            BindingSet = defaultBindingSet;
        }
    }
}
