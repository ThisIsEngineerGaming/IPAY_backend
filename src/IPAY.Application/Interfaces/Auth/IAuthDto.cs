using System.Threading.Tasks;

namespace IPAY.Application.Interfaces.Auth
{
    //authorization service related with dto's
    public interface IAuthDto<T> where T : class
    {
        Task<bool> Register(T dto);


    }
}

