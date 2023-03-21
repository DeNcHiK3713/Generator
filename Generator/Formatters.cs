using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{
    public static class Formatters
    {
        public static StringBuilder AppendPatchData(this StringBuilder hex, byte[] patchData)
        {
            if (patchData != null)
            {
                //hex.Capacity = hex.Length + patchData.Length * 3 + 1;
                hex.Append('\"');
                for (int i = 0, len = patchData.Length - 1; i < len; i++)
                {
                    hex.AppendFormat("{0:X2} ", patchData[i]);
                }
                hex.AppendFormat("{0:X2}\"", patchData[patchData.Length - 1]);
            }
            return hex;
        }
    }
}
