using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using StbImageSharp;

namespace ObrazkyJakCyp
{
    internal static class ContentLoader
    {
        private struct GifFrame
        {
            public Texture2D Frame;
            public int Delay;
        }

        private static Dictionary<string, PaintingContent> _cache = new Dictionary<string, PaintingContent>();
        internal static int _CurrIdx = 0;

        private static Texture2D TextureFromResult(ImageResult img)
        {
            int rowSize = img.Width * 4;
            var result = new byte[img.Data.LongLength];
            var tex = new Texture2D(img.Width, img.Height, TextureFormat.RGBA32, false);

            // Flip the texture
            for (int y = 0; y < img.Height; y++)
            {
                int rowIdx = y * rowSize;
                int destIdx = (img.Height - 1 - y) * rowSize;
                Buffer.BlockCopy(img.Data, rowIdx, result, destIdx, rowSize);
            }

            tex.LoadRawTextureData(result);
            tex.Apply();
            return tex;
        }

        public static PaintingContent GetNextContent()
        {
            var imgPath = Globals.ValidImages[_CurrIdx = (_CurrIdx + 1) % Globals.ValidImages.Count];
            if (_cache.TryGetValue(imgPath, out var content))
                return content;

            content = new PaintingContent();
            content.IsRawTexture = Path.GetFileNameWithoutExtension(imgPath).EndsWith("_tex");

            using (var img = File.OpenRead(imgPath))
                try
                {
                    // Try loading GIFs first
                    var gifRes = ImageResult.AnimatedGifFramesFromStream(img, ColorComponents.RedGreenBlueAlpha);
                    var frames = new List<GifFrame>();

                    content.FramesCnt = 0;
                    foreach (var frame in gifRes)
                        frames.Add(new GifFrame
                        {
                            Frame = TextureFromResult(frame),
                            Delay = frame.DelayInMs
                        });

                    var first = frames.First();
                    content.IsWide = first.Frame.width > first.Frame.height;
                    content.FramesCnt = frames.Count;
                    content.Delays = new int[content.FramesCnt];
                    content.Frames = new Texture2DArray(first.Frame.width, first.Frame.height, content.FramesCnt, first.Frame.format, false);
                    for (int i = 0; i < content.FramesCnt; i++)
                    {
                        var frame = frames[i];
                        content.Delays[i] = frame.Delay;
                        Graphics.CopyTexture(frame.Frame, 0, content.Frames, i);

                        UnityEngine.Object.Destroy(frame.Frame);
                    }
                } catch (Exception _)
                {
                    try
                    {
                        var imgRes = ImageResult.FromStream(img, ColorComponents.RedGreenBlueAlpha);
                        content.IsWide = imgRes.Width > imgRes.Height;
                        content.FramesCnt = 1;
                        content.Delays = null;
                        content.Frames = new Texture2DArray(imgRes.Width, imgRes.Height, 1, TextureFormat.RGBA32, false);

                        var tex = TextureFromResult(imgRes);
                        Graphics.CopyTexture(tex, 0, content.Frames, 0);
                        UnityEngine.Object.Destroy(tex);
                    } catch (Exception e)
                    {
                        Plugin.logger.LogError(e);
                        return null;
                    }
                }

            content.Frames.Apply();
            return _cache[imgPath] = content;
        }

        public static void Clear()
        {
            foreach (var kvp in _cache)
                UnityEngine.Object.DestroyImmediate(kvp.Value.Frames);

            _cache.Clear();
        }
    }
}
