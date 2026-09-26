namespace ExamTest.Application.Interfaces.Auth
{
    public interface IPasswordHashingService
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }
}
