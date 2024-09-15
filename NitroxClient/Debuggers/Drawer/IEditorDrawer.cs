namespace NitroxClient.Debuggers.Drawer;

public interface IEditorDrawer<T>
{
    T Draw(T target);
}

public interface IEditorDrawer<T, in TDrawOptions> : IEditorDrawer<T> where TDrawOptions : class
{
    T Draw(T target, TDrawOptions options);
}
