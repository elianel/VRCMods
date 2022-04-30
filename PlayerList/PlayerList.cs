using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnhollowerRuntimeLib.XrefScans;
using VRC;
using VRC.DataModel;
using VRC.SDKBase;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(PlayerList.PlayerList), nameof(PlayerList.PlayerList), "0.1.0", "elian", "github.com/elianel/VRCMods")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace PlayerList
{
    public class PlayerList : MelonMod
    {
        internal static MelonLogger.Instance Logger;
        internal static MelonPreferences_Category Category;
        internal static MelonPreferences_Entry<bool> DisplayRankColor;

        public override void OnApplicationStart()
        {
            Logger = LoggerInstance;
            Category = MelonPreferences.CreateCategory("Player List");
            DisplayRankColor = Category.CreateEntry("player_list", true, "Display Rank Colors", "Whether or not the playerlist displays rank colors.");
            UI.Setup().Start();

            var m0 = typeof(NetworkManager).GetMethod("Method_Public_Void_Player_0");
            var m1 = typeof(NetworkManager).GetMethod("Method_Public_Void_Player_1");
            var xreffed = XrefScanner.XrefScan(m0).ToArray();
            var found = false;
            for (int i = 0; i < xreffed.Length; i++)
            {
                if (xreffed[i].Type != XrefType.Global) continue;
                if (xreffed[i].ReadAsObject().ToString().Contains("OnPlayerJoin")) found = true;
            }
            _onPlayerJoined = Helper.Patch<OnPlayerJoined>(
                found ? m0 : m1,
                typeof(PlayerList).GetMethod(nameof(PlayerList.OnPlayerJoinedDetour), BindingFlags.Static | BindingFlags.NonPublic)
            );
            _onPlayerLeft = Helper.Patch<OnPlayerLeft>(
                found ? m1 : m0,
                typeof(PlayerList).GetMethod(nameof(PlayerList.OnPlayerLeftDetour), BindingFlags.Static | BindingFlags.NonPublic)
            );

            //sadly I couldn't find a better xref :( sooo loukylor's credit for the following..
            UI._QMSelectMethod = typeof(UserSelectionManager)
                .GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Void_APIUser_")
                && !method.Name.Contains("_PDM_")
                && method.IsUsedBy("Method_Public_Virtual_Final_New_Void_IUser_"));
        }
        private static IntPtr OnPlayerJoinedDetour(IntPtr instPtr, IntPtr playerPtr, IntPtr nativeMethodInfoPtr)
        {
            try { UI.AddPlayerToPList(UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<Player>(playerPtr)); }
            catch { Logger.Warning("Something exploded during OnPlayerJoinedDetour..."); }
            return _onPlayerJoined(instPtr, playerPtr, nativeMethodInfoPtr);
        }
        private static IntPtr OnPlayerLeftDetour(IntPtr instPtr, IntPtr playerPtr, IntPtr nativeMethodInfoPtr)
        {
            try { UI.DestroyEntry(UnhollowerSupport.Il2CppObjectPtrToIl2CppObject<Player>(playerPtr).field_Private_VRCPlayerApi_0.playerId); }
            catch { /* Explodes every time I leave a world Logger.Warning("Something exploded during OnPlayerLeftDetour..."); */ }
            return _onPlayerLeft(instPtr, playerPtr, nativeMethodInfoPtr);
        }
        private delegate IntPtr OnPlayerJoined(IntPtr instancePtr, IntPtr playerPtr, IntPtr nativeMethodInfoPtr);
        private delegate IntPtr OnPlayerLeft(IntPtr instancePtr, IntPtr playerPtr, IntPtr nativeMethodInfoPtr);
        private static OnPlayerJoined _onPlayerJoined;
        private static OnPlayerLeft _onPlayerLeft;
        public override void OnSceneWasLoaded(int buildIndex, string name)
        {
            if (buildIndex == -1) LocalLoad().Start();
        }
        internal static IEnumerator LocalLoad()
        {
            while (Networking.LocalPlayer == null) yield return null;
            UI.ClearEntries();
        }

    }
}

