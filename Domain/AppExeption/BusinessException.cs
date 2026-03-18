using ProjectHK251_Reworked.Domain.AppExeption;
namespace ProjectHK251_Reworked.Domain.AppException
{
    public class BusinessException : BaseAppException
    {

        public BusinessException(string message) : base(message, 400)
        { }

    }
}
