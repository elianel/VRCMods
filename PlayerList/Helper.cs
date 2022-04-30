using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib.XrefScans;
using VRC;
using VRC.Core;

namespace PlayerList
{
    internal static class Helper
    {
        #region PatchesAndXref
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
        public static bool IsUsedBy(this MethodBase method, string target)
        {
            var xref = XrefScanner.UsedBy(method).ToArray();
            for (var i = 0; i < xref.Length; i++)
            {
                var x = xref[i].TryResolve();
                if (x == null) continue;
                if (x.Name.Contains(target)) return true;
            }
            return false;
        }
        #endregion
        #region Extensions
        public static void Start(this IEnumerator enumer) => MelonCoroutines.Start(enumer);
        public static string GetRankColorHEX(this APIUser user)
        {
            if (user == null) return null;
            if (user.hasModerationPowers || user.hasSuperPowers) return MODERATOR;
            else if (user.tags.Contains("system_legend")) return LEGEND;
            else if (user.hasLegendTrustLevel) return VETERAN;
            else if (user.hasVeteranTrustLevel) return TRUSTED;
            else if (user.hasTrustedTrustLevel) return KNOWN;
            else if (user.hasKnownTrustLevel) return USER;
            else if (user.hasBasicTrustLevel) return NEW;
            else if (user.hasNegativeTrustLevel || user.hasVeryNegativeTrustLevel) return NUISANCE;
            else return VISITOR;
        }
        public static string GetRankString(this APIUser user)
        {
            if (user == null) return null;
            if (user.hasModerationPowers || user.hasSuperPowers) return "MODERATOR";
            else if (user.tags.Contains("system_legend")) return "LEGEND";
            else if (user.hasLegendTrustLevel) return "VETERAN";
            else if (user.hasVeteranTrustLevel) return "TRUSTED";
            else if (user.hasTrustedTrustLevel) return "KNOWN";
            else if (user.hasKnownTrustLevel) return "USER";
            else if (user.hasBasicTrustLevel) return "NEW";
            else if (user.hasNegativeTrustLevel || user.hasVeryNegativeTrustLevel) return "NUISANCE";
            else return "VISITOR";
        }
        public static string GetPlatform(this Player player)
        {
            if (player.field_Private_APIUser_0.last_platform == "android") return "Q";
            else return (player.field_Private_VRCPlayerApi_0.IsUserInVR() ? "VR" : "PC");
        }
        #endregion

        private const string VISITOR = "#cbcbcb";
        private const string NEW = "#1674f9";
        private const string USER = "#29b754";
        private const string KNOWN = "#e6713f";
        private const string TRUSTED = "#8c75c1";
        private const string VETERAN = "#fed000";
        private const string LEGEND = "#ff5991";
        private const string MODERATOR = "#fd2525";
        private const string NUISANCE = "#782f2f";
        private const string FRIEND = "#b18ffe";
    }
}
