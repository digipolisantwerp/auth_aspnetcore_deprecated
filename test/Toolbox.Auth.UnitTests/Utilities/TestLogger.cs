using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Toolbox.Auth.UnitTests
{
    public class TestLogger<T> : ILogger<T>
    {
        public List<string> LoggedMessages { get; set; }

        public TestLogger()
        {
            LoggedMessages = new List<string>();
        }

        public TestLogger(List<string> loggedMessages)
        {
            LoggedMessages = loggedMessages;
        }

        public IDisposable BeginScopeImpl(object state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log(LogLevel logLevel, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            LoggedMessages.Add($"{logLevel}, {state}");
        }

        public static TestLogger<T> CreateLogger()
        {
            return new TestLogger<T>();
        }

        public static TestLogger<T> CreateLogger(List<string> loggedMessages)
        {
            return new TestLogger<T>(loggedMessages);
        }
    }
}
