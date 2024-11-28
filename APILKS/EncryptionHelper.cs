using System.Text;

namespace APILKS
{
    public static class EncryptionHelper
    {
        public static string Encrypt(string plainText)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] + 1);
            }
            return Convert.ToBase64String(bytes);

        }
        public static string Decrypt(string encryptedText)
        {
            byte[] bytes = Convert.FromBase64String(encryptedText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] - 1);
            }
            return Encoding.UTF8.GetString(bytes);
        }
    }

}
