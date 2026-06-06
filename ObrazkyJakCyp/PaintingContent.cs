using System;

using UnityEngine;

namespace ObrazkyJakCyp
{
    internal class PaintingContent
    {
        public bool IsRawTexture = false;
        public bool IsWide = false;
        public int FramesCnt = 0;
        public int[] Delays = null;
        public Texture2DArray Frames = null;
    }
}
