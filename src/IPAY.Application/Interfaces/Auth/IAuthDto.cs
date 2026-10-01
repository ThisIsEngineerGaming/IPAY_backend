using System.Threading.Tasks;

namespace IPAY.Application.Interfaces.Auth
{
    public interface IAuthDto<T> where T : class
    {
        Task<bool> Register(T dto);


    }
}

