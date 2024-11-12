namespace NitroxClient.Debuggers.Drawer;

public interface IDrawer<in T>
{
    void Draw(T target);
}

public interface IDrawer<in T, in TDrawOptions> : IDrawer<T> where TDrawOptions : class
{
    void Draw(T target, TDrawOptions options);
}
