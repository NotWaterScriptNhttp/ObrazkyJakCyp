using System;
using System.IO;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace ObrazkyJakCyp
{
    [BepInPlugin("CecekMan.ObrazkyJakCyp", "ObrazkyJakCyp", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource logger { get; private set; }
        public static new PluginConfig Config { get; private set; } = null;
        internal static bool IsInitialized { get; private set; } = false;
        

        void Awake()
        {
            if (IsInitialized)
                return;

            logger = Logger;
            Config = new PluginConfig(base.Config);

            // Load painting template
            if (Globals.PaintingTemplate == null)
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var res = asm.GetManifestResourceStream("ObrazkyJakCyp.BasePainting.png"))
                {
                    if (res == null)
                    {
                        Logger.LogError("Failed to find BasePainting.png!");
                        return;
                    }

                    using (var ms = new MemoryStream())
                    {
                        res.CopyTo(ms);
                        Globals.PaintingTemplate = new Texture2D(1, 1);
                        Globals.PaintingTemplate.LoadImage(ms.ToArray());
                    }
                }
            }

            var dir = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, "Paintings"));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            else foreach (var imgPath in Directory.GetFiles(dir))
            {
                // Perform image validation
                try
                {
                    var imgData = File.ReadAllBytes(imgPath);
                    var tex = new Texture2D(1, 1);
                    if (!tex.LoadImage(imgData))
                        throw new Exception();

                    GameObject.DestroyImmediate(tex);
                    Globals.LoadedImages[imgPath] = null;
                } catch (Exception _) { Logger.LogError($"Painting: '{Path.GetFileName(imgPath)}' is not a valid image file!"); }
            }

            (new HarmonyLib.Harmony("CecekMan.ObrazkyJakCyp")).PatchAll();

            IsInitialized = true;
        }
    }
}
