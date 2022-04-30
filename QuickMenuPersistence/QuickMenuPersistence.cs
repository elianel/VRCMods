using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MelonLoader;
using UnityEngine.XR;
using VRC.SDKBase;
using VRC.UI;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(QuickMenuPersistence.QuickMenuPersistence), nameof(QuickMenuPersistence.QuickMenuPersistence), "0.1.0", "elian", "github.com/elianel/VRCMods")]
[assembly: MelonColor(ConsoleColor.DarkYellow)]
[assembly: MelonOptionalDependencies("UIExpansionKit")]

namespace QuickMenuPersistence
{
    public class QuickMenuPersistence : MelonMod
    {
        internal static MelonPreferences_Category Category;
        internal static MelonPreferences_Entry<bool> State;
        public override void OnApplicationStart()
        {
            if(!XRDevice.isPresent)
            {
                LoggerInstance.Warning("QuickMenu Persistence will not load as it is VR only.");
                return;
            }
            Category = MelonPreferences.CreateCategory("QuickMenuPersistence");
            State = Category.CreateEntry("quickmenu_persistence", false, "Persistent QuickMenu", "QuickMenu doesn't close when you move.");
            var qmclose = typeof(UIManagerImpl)
                .GetMethods()
                .First(method => method.Name.StartsWith("Method_Public_Void_Boolean_") && method.ContainsMethod("SetActive"));

            _quickMenuMethod = Helper.Patch<QMClose>(
                qmclose,
                typeof(QuickMenuPersistence).GetMethod(nameof(QuickMenuPersistence.QMCloseDetour), BindingFlags.Static | BindingFlags.NonPublic)
            );
        }
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (!XRDevice.isPresent || buildIndex != -1) return;
            MelonCoroutines.Start(LocalLoad());
        }
        internal static IEnumerator LocalLoad()
        {
            while (Networking.LocalPlayer == null) yield return null;
            State.Value = false;
        }
        private static QMClose _quickMenuMethod;
        private delegate IntPtr QMClose(IntPtr inst, bool value, IntPtr nativeM);
        private static IntPtr QMCloseDetour(IntPtr inst, bool value, IntPtr nativeM) => State.Value ? IntPtr.Zero : _quickMenuMethod(inst, value, nativeM);
    }
}
