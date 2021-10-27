namespace GraphRunner
{
    public interface ILogger
    {
        public void WriteLine(string format, params object[] obj);
        public void Write(string format, params object[] obj);
    }
}