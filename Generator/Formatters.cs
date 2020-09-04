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
                //hex.Capacity = hex.Length + patchData.Length * 6 + 2;
                hex.Append("{ ");
                for (int i = 0, len = patchData.Length - 1; i < len; i++)
                {
                    hex.AppendFormat("0x{0:x2}, ", patchData[i]);
                }
                hex.AppendFormat("0x{0:x2} }}", patchData[patchData.Length - 1]);
            }
            return hex;
        }
    }
}
