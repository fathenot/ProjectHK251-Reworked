namespace ProjectHK251_Reworked.Domain.AppExeption
{
    public class SystemException: BaseAppException
    {
        public SystemException(string message): base(message, 500) { }
    }
}
