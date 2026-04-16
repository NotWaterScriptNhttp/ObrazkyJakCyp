using System;

using HarmonyLib;
using Unity.Netcode;

namespace ObrazkyJakCyp.Patches
{
    [HarmonyPatch]
    internal static class RoundManagerPatches
    {
        [HarmonyPatch(typeof(RoundManager), "SyncScrapValuesClientRpc")]
        [HarmonyPostfix]
        private static void OnSyncScrapValues(NetworkObjectReference[] spawnedScrap)
        {
            if (!Plugin.IsInitialized)
                return;

            foreach (var scrap in spawnedScrap)
                if (scrap.TryGet(out var nobj))
                {
                    var obj = nobj.GetComponent<GrabbableObject>();
                    if (!obj.IsPainting())
                        continue;

                    Utils.ApplyTextureToPainting(obj);
                }
        }

        [HarmonyPatch(typeof(RoundManager), "GenerateNewLevelClientRpc")]
        [HarmonyPrefix]
        private static void OnGenerateNewLevel(int randomSeed)
        {
            Globals.GRandom = new Random(randomSeed + 300);
        }
    }
}
