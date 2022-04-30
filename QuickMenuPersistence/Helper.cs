using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib.XrefScans;

namespace QuickMenuPersistence
{
    internal static class Helper
    {
        public static unsafe T Patch<T>(MethodInfo targetMethod, MethodInfo patch) where T : Delegate
        {
            var method = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(targetMethod).GetValue(null);
            MelonUtils.NativeHookAttach((IntPtr)(&method), patch!.MethodHandle.GetFunctionPointer());
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        public static bool ContainsMethod(this MethodBase method, string target)
        {
            var xref = XrefScanner.XrefScan(method).ToArray();
            for (var i = 0; i < xref.Length; i++)
            {
                var resolved = xref[i].TryResolve();
                if (resolved == null) continue;
                if (resolved.Name.Contains(target)) return true;
            }
            return false;
        }
    }
}
