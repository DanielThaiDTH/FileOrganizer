using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Security;

namespace FileOrganizerUI.CodeBehind
{
    public class NaturalOrderComparer : IComparer<string>
    {
        [SuppressUnmanagedCodeSecurity]
        internal static class Win32Methods
        {
            [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
            public static extern int StrCmpLogicalW(string psz1, string psz2);
        }
        public int Compare(string a, string b)
        {
            return Win32Methods.StrCmpLogicalW(a, b);
        }
    }
}
