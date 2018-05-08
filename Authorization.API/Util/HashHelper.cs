using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace Authorization.API.Util
{
    public interface IHashHelper
    {
        bool CompareStringToSHA256(string str, byte[] hash256);
        byte[] ComputeSha256FromString(string str);
    }

    public class HashHelper : IHashHelper
    {
        public byte[] ComputeSha256FromString(String str)
        {
            byte[] passwordAsHash256;
            using (var sha256 = SHA256.Create())
            {
                passwordAsHash256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
            return passwordAsHash256;
        }

        public bool CompareStringToSHA256(String str, byte[] hash256)
        {
            byte[] stringAsSha256;
            using (var sha256 = SHA256.Create())
            {
                stringAsSha256 = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
            return StructuralComparisons.StructuralEqualityComparer.Equals(stringAsSha256, hash256);
        }

    }
}
