using System.Threading.Tasks;

namespace ExamTest.Application.Interfaces.Auth
{
    public interface IAuthDto<T> where T : class
    {
        Task<bool> Register(T dto);


    }
}

