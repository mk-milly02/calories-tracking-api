using System.Security.Cryptography;
using System.Text;

namespace calories_tracking.domain;

public static class Security
{
    public static string GenerateSalt()
    {
        //"using" ensures that RNG is well disposed
        using RandomNumberGenerator generator = RandomNumberGenerator.Create();
        byte[] randomNumberBytes = new byte[32];
        generator.GetBytes(randomNumberBytes);
        return Convert.ToBase64String(randomNumberBytes);
    }

    public static string GenerateSaltedPassword(string password, string salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
        byte[] saltedPasswordBytes = new byte[saltBytes.Length + passwordBytes.Length];

        Buffer.BlockCopy(passwordBytes, 0, saltedPasswordBytes, 0, passwordBytes.Length);
        Buffer.BlockCopy(saltBytes, 0, saltedPasswordBytes, passwordBytes.Length, saltBytes.Length);
        return Convert.ToBase64String(saltedPasswordBytes);
    }
}
