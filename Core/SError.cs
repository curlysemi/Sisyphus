namespace Sisyphus.Core
{
    public class SError
    {
        private string _message { get; set; }

        public SError()
        {
            _message = "An unknown error occurred.";
        }

        public SError(string message)
        {
            _message = message;
        }

        public static implicit operator string(SError erorr)
        {
            return erorr._message;
        }

        public static implicit operator SError(string message)
        {
            return new SError(message);
        }
    }
}
