using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OutputLogManagerNEW.Utils
{
    public class IniFile
    {
        private readonly string path;

        public IniFile(string iniPath)
        {
            path = iniPath;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string defaultValue,
            StringBuilder returnValue,
            int size,
            string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(
            string section,
            string key,
            string value,
            string filePath);

        public string? Read(string section, string key)
        {
            var buffer = new StringBuilder(512);
            int bytesReturned = GetPrivateProfileString(section, key, "", buffer, buffer.Capacity, path);

            if (bytesReturned > 0)
                return buffer.ToString();

            return null;
        }

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }
    }
}
