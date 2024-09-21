using Microsoft.AspNetCore.Identity;

namespace CanamDistributors.Helper
{
    public class PasswordHelper
    {
        private static readonly PasswordHasher<object> _passwordHasher = new PasswordHasher<object>();

        public static string HashPassword(string password)
        {
            return _passwordHasher.HashPassword(null, password);
        }

        public static PasswordVerificationResult VerifyPassword(string hashedPassword, string providedPassword)
        {
            return _passwordHasher.VerifyHashedPassword(null, hashedPassword, providedPassword);
        }
    }

}
