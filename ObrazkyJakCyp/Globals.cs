using System;
using System.Collections.Generic;

using UnityEngine;

namespace ObrazkyJakCyp
{
    internal static class Globals
    {
        public static System.Random GRandom = new System.Random();

        public static AssetBundle Bundle = null;
        public static Shader PaintingShader = null;

        public static List<string> ValidImages = new List<string>();
        public static HashSet<int> Paintings = new HashSet<int>();
    }
}
