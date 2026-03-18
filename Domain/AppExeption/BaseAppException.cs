namespace ProjectHK251_Reworked.Domain.AppExeption
{
    public abstract class BaseAppException : Exception
    {
        public int StatusCode { get; }
        protected BaseAppException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;

        }
    }
}
