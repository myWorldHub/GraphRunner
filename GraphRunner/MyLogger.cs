using System;

namespace GraphRunner
{
    public class MyLogger : ILogger
    {
        public void WriteLine(string format, params object[] obj)
        {
            Console.WriteLine(format, arg: obj);
        }

        public void Write(string format, params object[] obj)
        {
            Console.Write(format, arg: obj);
        }
    }
}