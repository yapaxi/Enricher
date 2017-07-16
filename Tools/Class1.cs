using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class StringTools
    {
        public static byte[] GetBytes(string str)
        {
            var arr = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, arr, 0, arr.Length);
            return arr;
        }

        public static string GetString(byte[] bytes)
        {
            var arr = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, arr, 0, bytes.Length);
            return new string(arr);
        }
    }
}
