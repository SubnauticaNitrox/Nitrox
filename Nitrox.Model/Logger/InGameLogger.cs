namespace Nitrox.Model.Logger
{
    public interface InGameLogger
    {
        void Log(object message);
        void Log(string message);
    }
}
