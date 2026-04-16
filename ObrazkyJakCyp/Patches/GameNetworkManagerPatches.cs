using System;

using HarmonyLib;

namespace ObrazkyJakCyp.Patches
{
    [HarmonyPatch]
    internal static class GameNetworkManagerPatches
    {
        [HarmonyPatch(typeof(GameNetworkManager), "ResetGameValuesToDefault")]
        [HarmonyPostfix]
        private static void OnResetGameValues()
        {
            if (!Plugin.IsInitialized)
                return;

            Globals.PaintingCache.Clear();
        }
    }
}
