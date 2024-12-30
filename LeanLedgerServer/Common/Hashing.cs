namespace LeanLedgerServer.Common;

using System.Security.Cryptography;
using System.Text;

public static class Hashing {
    public static string HashString(string input) {
        // Convert the input string to a byte array and compute the hash.
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        var sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data
        // and format each one as a hexadecimal string.
        foreach (var b in data) {
            sBuilder.Append(b.ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    // Verify a hash against a string.
    public static bool IsValidHash(string input, string hash) {
        // Hash the input.
        var hashOfInput = HashString(input);

        // Create a StringComparer an compare the hashes.
        var comparer = StringComparer.OrdinalIgnoreCase;

        return comparer.Compare(hashOfInput, hash) == 0;
    }
}
