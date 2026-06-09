using System;
using System.Collections.Generic;

using UnityEngine;

using ObrazkyJakCyp.Components;

namespace ObrazkyJakCyp
{
    internal static class Globals
    {
        public static System.Random GRandom = new System.Random();

        public static AssetBundle Bundle = null;
        public static Shader PaintingShader = null;

        public static List<string> ValidImages = new List<string>();
        public static Dictionary<int, CustomPainting> Paintings = new Dictionary<int, CustomPainting>();
    }
}
