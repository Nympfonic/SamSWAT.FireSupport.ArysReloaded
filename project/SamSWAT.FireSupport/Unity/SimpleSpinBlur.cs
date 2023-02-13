//CREDIT TO AiKodex
//https://assetstore.unity.com/packages/tools/integration/simple-spin-blur-202273

using System.Collections.Generic;
using UnityEngine;

namespace SamSWAT.FireSupport.Unity
{
    public class SimpleSpinBlur : MonoBehaviour
    {
        private Mesh _ssbMesh;
        private Material _ssbMaterial;
        private Queue<Quaternion> rotationQueue = new Queue<Quaternion>();
        
        [Range(1, 128)] [Tooltip("Motion Blur Amount")]
        public int shutterSpeed = 4;

        [Range(1, 50)] [Tooltip("Motion Blur Samples")]
        public int samples = 8;

        [Range(-0.1f, 0.1f)] [Tooltip("Motion Blur Opacity")]
        public float alphaOffset;

        [Tooltip("[Optimization] Enables material's GPU Instancing property")]
        public bool enableGPUInstancing;

        [Tooltip("[Optimization] Angular velocity threshold value before which the effects will not be rendered.")]
        public float angularVelocityCutoff;

        private void Start()
        {
            _ssbMesh = GetComponent<MeshFilter>().mesh;
            _ssbMaterial = GetComponent<MeshRenderer>().sharedMaterial;
            _ssbMaterial.enableInstancing = enableGPUInstancing;
        }

        private void Update()
        {
            if (rotationQueue.Count >= shutterSpeed)
            {
                rotationQueue.Dequeue();
                //Second Dequeue to reduce queue size
                if (rotationQueue.Count >= shutterSpeed)
                {
                    rotationQueue.Dequeue();
                }
            }

            rotationQueue.Enqueue(transform.rotation);
            if (Quaternion.Angle(transform.rotation, rotationQueue.Peek()) / shutterSpeed >= angularVelocityCutoff)
            {
                for (int i = 0; i <= samples; i++)
                {
                    Graphics.DrawMesh(_ssbMesh, transform.position,
                        Quaternion.Lerp(rotationQueue.Peek(), transform.rotation, i / (float) samples),
                        _ssbMaterial, 0, null, 0);
                }

                var tempColor = new Color(_ssbMaterial.color.r, _ssbMaterial.color.g, _ssbMaterial.color.b,
                    Mathf.Abs((2 / (float) samples) + alphaOffset));
                _ssbMaterial.color = tempColor;
            }
            else
            {
                if (!(_ssbMaterial.color.a < 1)) return;
                var tempColor = new Color(_ssbMaterial.color.r, _ssbMaterial.color.g, _ssbMaterial.color.b, 1);
                _ssbMaterial.color = tempColor;
            }
        }
    }
}