using System;
using System.Linq;

using HarmonyLib;

namespace ObrazkyJakCyp.Patches
{
    [HarmonyPatch]
    internal static class GrabbableObjectPatches
    {
        [HarmonyPatch(typeof(GrabbableObject), "Start")]
        [HarmonyPrefix]
        private static void OnStart(GrabbableObject __instance)
        {
            if (!Plugin.IsInitialized || !__instance.IsPainting())
                return;

            Utils.ApplyTextureToPainting(__instance);
        }
    }
}
