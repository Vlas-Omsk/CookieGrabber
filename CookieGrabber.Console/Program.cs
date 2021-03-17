using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CookieGrabber.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Drawing
            System.Console.WindowWidth = 160;
            System.Console.WindowHeight = 40;

            var columns = new int[]{ 9, 20, 10, 20, System.Console.WindowWidth - 26 - 23 * 2 };
            var columnHeaders = new string[] { "Browser", "Host", "Path", "Name", "Value" };

            for (var i = 0; i < columns.Length; i++)
            {
                var left = (columns[i] - columnHeaders[i].Length) / 2;
                var right = columns[i] - columnHeaders[i].Length - left;

                System.Console.Write(new string(' ', left) + columnHeaders[i] + new string(' ', right) + (i == columns.Length - 1 ? "" : " | "));
            }
            System.Console.WriteLine();
            for (var i = 0; i < columns.Length; i++)
                System.Console.Write(new string('=', columns[i]) + (i == columns.Length - 1 ? "" : " | "));
            System.Console.WriteLine();
            #endregion

            foreach (var cookie in CookieTool.GetAllCookies())
                System.Console.WriteLine($"{{0,-{columns[0]}}} | {{1,-{columns[1]}}} | {{2,-{columns[2]}}} | {{3,-{columns[3]}}} | {{4,-{columns[4]}}}",
                    Trim(cookie.Browser, columns[0]),
                    Trim(cookie.Host, columns[1]), 
                    Trim(cookie.Path, columns[2]), 
                    Trim(cookie.Name, columns[3]),
                    Trim(cookie.Value, columns[4]));

            System.Console.ReadLine();
        }

        static string Trim(string str, int length)
        {
            return str.Substring(0, str.Length < length ? str.Length : length);
        }
    }
}
