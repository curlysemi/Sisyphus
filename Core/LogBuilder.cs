using System.Text;

namespace Sisyphus.Core
{
    internal class LogBuilder
    {
        private StringBuilder _sb { get; set; }
        private bool _isVerbose { get; set; }
        public LogBuilder(bool isVerbose)
        {
            _isVerbose = isVerbose;
            _sb = new StringBuilder();
        }

        public void Log(string message)
        {
            _sb.AppendLine(message);
        }

        public void NL()
        {
            _sb.AppendLine(string.Empty);
        }

        public void LogNoLine(string message)
        {
            _sb.Append(message);
        }

        public void Vlog(string message)
        {
            if (_isVerbose)
            {
                Log(message);
            }
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
