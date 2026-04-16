using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace ObrazkyJakCyp
{
    internal static class Utils
    {
        private static readonly Vector2Int _startPoint = new Vector2Int(527, 37);
        private static readonly Vector2Int _endPoint = new Vector2Int(1015, 687);
        private static readonly Vector2Int _targetSize;

        static Utils()
        {
            _targetSize = _endPoint - _startPoint;
        }

        public static string GetRandomImage()
        {
            if (Globals.LoadedImages.Count == 0)
                return null;

            var keys = Globals.LoadedImages.Keys.ToArray();
            return keys[Globals.GRandom.Next(keys.Length - 1)];
        }

        public static Texture2D ResizeTexture(Texture2D tex, int w, int h)
        {
            RenderTexture rt = RenderTexture.GetTemporary(w, h);
            Graphics.Blit(tex, rt);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;

            var final = new Texture2D(w, h);
            final.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            final.Apply();

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return final;
        }
        public static void ApplyTextureToPainting(GrabbableObject p)
        {
            if (Globals.PaintingCache.TryGetValue(p.GetInstanceID(), out var final))
                goto APPLY_TEXTURE;

            var texFile = GetRandomImage();
            if (texFile == null)
                return;

            if (Globals.LoadedImages.TryGetValue(texFile, out var loadedTex) && loadedTex != null)
                goto CREATE_MAT;

            var texData = File.ReadAllBytes(texFile);
            var tex = new Texture2D(1, 1);
            if (!tex.LoadImage(texData))
            {
                Plugin.logger.LogError($"Failed to load image: {Path.GetFileName(texFile)} with size: 0x{texData.Length:X8}");
                return; // Validity check passed, but this didn't so we just exit
            }

            if (tex.width > tex.height)
            {
                var pixels = tex.GetPixels();
                var tpixles = new Color[pixels.Length];

                for (int x = 0; x < tex.width; x++)
                    for (int y = 0; y < tex.height; y++)
                        tpixles[(tex.width - x - 1) * tex.height + y] = pixels[y * tex.width + x];

                tex.Reinitialize(tex.height, tex.width);
                tex.SetPixels(tpixles);
                tex.Apply();
            }

            // Resize the image
            var resized = ResizeTexture(tex, _targetSize.x, _targetSize.y);
            GameObject.DestroyImmediate(tex);
            tex = resized;

            var texPixels = tex.GetPixels();
            var tempPixels = Globals.PaintingTemplate.GetPixels();
            loadedTex = new Texture2D(Globals.PaintingTemplate.width, Globals.PaintingTemplate.height, Globals.PaintingTemplate.format, false);

            for (int x = 0; x < tex.width; x++)
                for (int y = 0; y < tex.height; y++)
                    tempPixels[_startPoint.x + x + ((loadedTex.height - _endPoint.y) + y) * loadedTex.width] = texPixels[x + y * tex.width];

            loadedTex.SetPixels(tempPixels);
            loadedTex.Apply();

            GameObject.DestroyImmediate(tex);
            Globals.LoadedImages[texFile] = loadedTex;

        CREATE_MAT:
            final = new Material(p.itemProperties.materialVariants[0])
            {
                mainTexture = loadedTex
            };

        APPLY_TEXTURE:
            p.gameObject.GetComponent<MeshRenderer>().sharedMaterial = final;
            Globals.PaintingCache[p.GetInstanceID()] = final;
        }
    }
}
