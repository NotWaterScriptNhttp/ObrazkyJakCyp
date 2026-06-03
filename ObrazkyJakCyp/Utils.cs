using System;
using System.IO;
using System.Linq;

using UnityEngine;

using StbImageSharp;
using System.Collections.Generic;

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

        public static Texture2D CreateTextureFromResult(ImageResult img, bool rotateAllowed)
        {
            Texture2D tex = new Texture2D(img.Width, img.Height, TextureFormat.RGBA32, false);
            //TODO: Rotate the image here
            /*if (!rotateAllowed || img.Height > img.Width)
                tex = new Texture2D(img.Width, img.Height, TextureFormat.RGB24, false);
            else tex = new Texture2D(img.Height, img.Width, TextureFormat.RGB24, false);*/

            int rowSize = img.Width * 4;
            byte[] modified = new byte[img.Data.LongLength];
            for (int y = 0; y < img.Height; y++)
            {
                int rowIdx = y * rowSize;
                int destIdx = (img.Height - 1 - y) * rowSize;
                Buffer.BlockCopy(img.Data, rowIdx, modified, destIdx, rowSize);
            }

            tex.LoadRawTextureData(modified);
            tex.Apply();
            return tex;
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
            if (texFile == null) // No image was provided by the user
                return;

            if (Globals.LoadedImages.TryGetValue(texFile, out var loadedTex) && loadedTex != null)
                goto CREATE_MAT;

            Texture2D tex = null;
            bool canRotate = !Path.GetFileNameWithoutExtension(texFile).EndsWith("_tex");
            using (var img = File.OpenRead(texFile))
            {
                try
                {
                    var gifRes = ImageResult.AnimatedGifFramesFromStream(img, ColorComponents.RedGreenBlueAlpha);
                    foreach (var frame in gifRes)
                        continue;

                    tex = CreateTextureFromResult(gifRes.First(), canRotate);
                    Plugin.logger.LogWarning("Loading GIF!");

                } catch (Exception _) // File is not a gif
                {
                    tex = CreateTextureFromResult(ImageResult.FromStream(img, ColorComponents.RedGreenBlueAlpha), canRotate);
                }
            }

            // Check for '_tex' suffix, that tells us the image is a texture, the user provided
            if (canRotate)
            {
                // Flip the texture by 90°
                if (tex.width > tex.height)
                {
                    var pixels = tex.GetPixels();
                    var tpixles = new Color[pixels.Length];

                    for (int x = 0; x < tex.width; x++)
                        for (int y = 0; y < tex.height; y++)
                            tpixles[x * tex.height + (tex.height - 1 - y)] = pixels[y * tex.width + x];

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

                // Replace placeholder with randomly picked image
                for (int x = 0; x < tex.width; x++)
                    for (int y = 0; y < tex.height; y++)
                        tempPixels[_startPoint.x + x + ((loadedTex.height - _endPoint.y) + y) * loadedTex.width] = texPixels[x + y * tex.width];

                loadedTex.SetPixels(tempPixels);
                loadedTex.Apply();

                GameObject.DestroyImmediate(tex); // Resized image isn't used anymore
            }
            else loadedTex = tex;
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
