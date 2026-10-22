using IPAY.Application.Interfaces.Auth;
using System.Security.Cryptography;

namespace IPAY.Application.Services.Auth
{
    //password service
    public sealed class PasswordHashingService : IPasswordHashingService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 210_000;
        private const string Algorithm = "PBKDF2-SHA256";

        //making a hash
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Algorithm}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";//returning a hashed service
        }
        //checking if password suits hash template
        public bool Verify(string password, string passwordHash)
        {
            var parts = passwordHash.Split('$');
            if (parts.Length != 4 || parts[0] != Algorithm ||
                !int.TryParse(parts[1], out var iterations) || iterations < 1)
            {
                return false;
            }
            
            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)//if it's false throwing a error related with format
            {
                return false;
            }
        }
    }
}
