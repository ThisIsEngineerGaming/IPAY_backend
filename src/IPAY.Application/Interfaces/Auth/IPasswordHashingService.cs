namespace IPAY.Application.Interfaces.Auth
{
    //interface realted with password-hashing
    public interface IPasswordHashingService
    {
        string Hash(string password);
        bool Verify(string password, string passwordHash);
    }
}
