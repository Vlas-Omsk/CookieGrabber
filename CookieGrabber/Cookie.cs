using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CookieGrabber
{
    public class Cookie
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Host { get; private set; }
        public string Path { get; private set; }
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsSecure { get; private set; }
        public string CookieFilePath { get; internal set; }
        public string Browser => getBrowserName();

        public Cookie(string name, string value, DateTime createdAt, DateTime expiresAt, string host, string path, bool isSecure)
        {
            Name = name;
            Value = value;
            CreatedAt = createdAt;
            ExpiresAt = expiresAt;
            Host = host;
            Path = path;
            IsSecure = isSecure;
        }

        // WTF?
        string getBrowserName()
        {
            try
            {
                var browser = "";
                if (CookieFilePath.EndsWith(CookieTool.ChromiumUserProfile + CookieTool.ChromiumCookiePath))
                    browser = CookieFilePath.Replace(CookieTool.ChromiumUserProfile + CookieTool.ChromiumCookiePath, null);
                else if (CookieFilePath.EndsWith(CookieTool.GeckoCookiePath) && (browser = Directory.GetParent(CookieFilePath.Replace(CookieTool.GeckoCookiePath, null)).FullName).EndsWith(CookieTool.GeckoUserProfiles))
                    browser = Directory.GetParent(browser).FullName;
                else
                    return null;

                return browser.Substring(browser.LastIndexOf("\\") + 1);
            }
            catch { return null; }
        }

        public override string ToString()
        {
            return $"{Name} = {Value}; CreatedAt={CreatedAt}; ExpiresAt={ExpiresAt}";
        }
    }
}
