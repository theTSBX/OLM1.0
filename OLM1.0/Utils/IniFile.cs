using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace OutputLogManagerNEW.Utils
{
    public class IniFile
    {
        private readonly string path;

        public IniFile(string path)
        {
            this.path = path;
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue,
            StringBuilder retVal, int size, string filePath);

        public string Read(string key, string section)
        {
            var retVal = new StringBuilder(1024);
            GetPrivateProfileString(section, key, "", retVal, 1024, path);
            return retVal.ToString();
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, path);
        }


    }
}
