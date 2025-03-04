namespace Marelli.Business.Exceptions
{
    public class InvalidPasswordException : Exception
    {

        public InvalidPasswordException(string message)
            : base(message)
        {
        }
    }

}
