using System;
using System.Collections.Generic;

using UnityEngine;

namespace ObrazkyJakCyp
{
    internal static class Globals
    {
        public static System.Random GRandom = new System.Random();

        public static Texture2D PaintingTemplate = null;
        public static Dictionary<string, Texture2D> LoadedImages = new Dictionary<string, Texture2D>();
        public static Dictionary<int, Material> PaintingCache = new Dictionary<int, Material>();
    }
}
