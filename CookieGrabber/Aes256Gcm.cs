using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookieGrabber.BouncyCastle;

namespace CookieGrabber
{
    internal static class Aes256Gcm
    {
        public static string decrypt(byte[] encryptedBytes, KeyParameter key, byte[] iv)
        {
            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine());
            AeadParameters parameters = new AeadParameters(key, 128, iv, null);

            cipher.Init(false, parameters);
            byte[] plainBytes = new byte[cipher.GetOutputSize(encryptedBytes.Length)];
            Int32 retLen = cipher.ProcessBytes(encryptedBytes, 0, encryptedBytes.Length, plainBytes, 0);
            cipher.DoFinal(plainBytes, retLen);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
