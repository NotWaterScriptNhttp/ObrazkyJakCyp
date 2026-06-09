using System;

using UnityEngine;

namespace ObrazkyJakCyp.Components
{
    internal class CustomPainting : MonoBehaviour
    {
        private GrabbableObject Obj;
        private MeshRenderer Renderer;

        private bool IsAnimation = false;
        private float Timer = 0;
        private int Frame = 0;

        public Material Mat;
        public PaintingContent Content;

        void Awake()
        {
            Content = ContentLoader.GetNextContent();
            if (Content == null)
            {
                Plugin.logger.LogError("Failed to get an image for painting!");
                return;
            }

            IsAnimation = Content.Delays != null;

            Obj = GetComponent<GrabbableObject>();

            Mat = new Material(Obj.itemProperties.materialVariants[0]);
            Mat.shader = Globals.PaintingShader;
            Mat.SetTexture("_Images", Content.Frames);
            Mat.SetInt("_Index", 0);
            Mat.SetInt("_Rotate", Content.IsWide ? 1 : 0);
            Mat.SetInt("_IsTexture", Content.IsRawTexture ? 1 : 0);
        }
        void Update()
        {
            if (!IsAnimation || !Renderer)
                return;

            float delay = Content.Delays[Frame] / 1000f;

            Timer += Time.deltaTime;
            if (Timer >= delay)
            {
                Timer -= delay;
                Frame = (Frame + 1) % Content.FramesCnt;
                Renderer.material.SetInt("_Index", Frame);
            }
        }

        public void SetupPainting()
        {
            Renderer = GetComponent<MeshRenderer>();
            if (!Renderer)
                return;

            Renderer.material = Mat;
        }
    }
}
