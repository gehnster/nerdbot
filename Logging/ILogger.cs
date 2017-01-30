namespace Logging
{
    public interface ILogger
    {
        void InfoLogging(string i);
        void DebugLogging(string d);
        void ErrorLogging(string e);
    }
}
