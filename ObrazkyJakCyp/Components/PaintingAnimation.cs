using System;

using UnityEngine;

namespace ObrazkyJakCyp.Components
{
    internal class PaintingAnimation : MonoBehaviour
    {
        private GrabbableObject Obj;
        private float Timer = 0;
        private int Frame = 0;

        public PaintingContent Content;

        void Start()
        {
            Obj = GetComponent<GrabbableObject>();
        }

        void Update()
        {
            float delay = Content.Delays[Frame] / 1000f;

            Timer += Time.deltaTime;
            if (Timer >= delay)
            {
                Timer -= delay;
                Frame = (Frame + 1) % Content.FramesCnt;
                Obj.GetComponent<MeshRenderer>().material.SetInt("_Index", Frame);
            }
        }
    }
}
