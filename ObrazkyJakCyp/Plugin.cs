using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Logging;

using UnityEngine;

using StbImageSharp;

namespace ObrazkyJakCyp
{
    [BepInPlugin("CecekMan.ObrazkyJakCyp", "ObrazkyJakCyp", "1.0.3")]
    public class Plugin : BaseUnityPlugin
    {
        private const int IMAGE_BLOCK_LEN = 64;

        public static ManualLogSource logger { get; private set; }
        public static new PluginConfig Config { get; private set; } = null;
        internal static bool IsInitialized { get; private set; } = false;

        IEnumerable<string> GetImage()
        {
            var dir = Config.Directory;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            List<string> files = new List<string>();
            Stack<string> folders = new Stack<string>();
            folders.Push(dir);

            while (folders.Count > 0)
            {
                var curr = folders.Pop();
                foreach (var fold in Directory.GetDirectories(curr))
                    folders.Push(fold);

                foreach (var file in Directory.GetFiles(curr))
                    files.Add(file);
            }

            if (files.Count <= Config.MaxImages)
                foreach (var file in files)
                    yield return file;
            else while (files.Count > 0)
            {
                int idx = files.Count == 1 ? 0 : Globals.GRandom.Next(files.Count);
                string[] blck = new string[IMAGE_BLOCK_LEN];
                for (int i = 0; i < IMAGE_BLOCK_LEN; i++)
                    blck[i] = files[(idx + i) % files.Count];

                int idx2 = Globals.GRandom.Next(IMAGE_BLOCK_LEN);
                yield return blck[idx2];
                files.RemoveAt((idx + idx2) % files.Count);
            }

            yield return null;
        }

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

            new Thread(() =>
            {
                int maxVal = Config.MaxImages;
                foreach (var img in GetImage())
                {
                    if (img == null || (maxVal != -1 && maxVal <= Globals.LoadedImages.Count))
                        break;

                    // Validate image
                    using (var file = File.OpenRead(img))
                        if (ImageInfo.FromStream(file) != null)
                            Globals.LoadedImages[img] = null;
                        else Logger.LogError($"Painting: '{Path.GetFileName(img)}' is not a valid image file!");
                }

                Logger.LogInfo($"Loaded {Globals.LoadedImages.Count} images!");
            }).Start();

            (new HarmonyLib.Harmony("CecekMan.ObrazkyJakCyp")).PatchAll();

            IsInitialized = true;
        }
    }
}
