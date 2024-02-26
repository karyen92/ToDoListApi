using System.Security.Cryptography;
using System.Text;

namespace ToDoListApi.Extensions;

public static class HashingExtension
{
    public static string Sha256Hash(string input, string salt)
    {
        var sb = new StringBuilder();

        using (var hash = SHA256.Create())
        {
            var enc = Encoding.UTF8;
            var result = hash.ComputeHash(enc.GetBytes(input+salt));

            foreach (var b in result)
                sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}