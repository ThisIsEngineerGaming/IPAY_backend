using System.Threading.Tasks;

namespace ExamTest.Application.Interfaces.Auth
{
    public interface IAuth<T> where T : class
    {
        Task<bool> Register(T dto);
    }
}

