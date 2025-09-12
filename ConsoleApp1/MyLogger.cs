using EventLogger;

namespace ConsoleApp1
{
    internal class MyLogger : IGenericEventDispatcherLogger
    {
        public void LogError(string message)
        {
            Console.WriteLine(message);
        }

        public void LogError(string message, Exception exception)
        {
            Console.WriteLine($"Error: {exception}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine(message);
        }
    }
}
