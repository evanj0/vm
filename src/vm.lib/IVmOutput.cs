using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vm.lib
{
    public interface IVmOutput
    {
        public void WriteLine(string s);

        public void Write(string s);

        public void WriteBytes(byte[] bytes);
    }

    public class ConsoleOutput : IVmOutput
    {
        private readonly Stream _stream = Console.OpenStandardOutput();

        public void WriteLine(string s)
        {
            Console.WriteLine(s);
        }

        public void Write(string s)
        {
            Console.Write(s);
        }

        public void WriteBytes(byte[] bytes)
        {
            _stream.Write(bytes);
        }
    }

    public class LoggedOutput : IVmOutput
    {
        public LoggedOutput()
        {
            _sb = new StringBuilder();
        }

        private StringBuilder _sb;

        public void WriteLine(string s)
        {
            _sb.AppendLine(s);
        }

        public void Write(string s)
        {
            _sb.Append(s);
        }

        public void WriteBytes(byte[] bytes)
        {
            _sb.Append(Encoding.UTF8.GetString(bytes));
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
