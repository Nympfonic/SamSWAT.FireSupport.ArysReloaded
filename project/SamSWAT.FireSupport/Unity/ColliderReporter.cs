using System;
using UnityEngine;

namespace SamSWAT.FireSupport.Unity
{
    public class ColliderReporter : MonoBehaviour
    {
        [NonSerialized] 
        public bool HasCollision;
        private BoxCollider[] _colliders;
        private Collider[] _intersectedColliders;

        private void Start()
        {
            _intersectedColliders = new Collider[5];
            _colliders = GetComponents<BoxCollider>();
        }
        
        private void Update()
        {
            foreach (var col in _colliders)
            {
                var colTransform = col.transform;
                var colRotation = colTransform.rotation;
                var center = colTransform.position + colRotation * col.center;
                var extents = Vector3.Scale(col.size * 0.5f, colTransform.lossyScale);
                
                var hits = Physics.OverlapBoxNonAlloc(
                    center,
                    extents,
                    _intersectedColliders,
                    colRotation,
                    LayerMask.GetMask("LowPolyCollider", "HighPolyCollider"),
                    QueryTriggerInteraction.Ignore);

                if (hits > 0)
                {
                    HasCollision = true;
                    break;
                }

                HasCollision = false;
            }
        }
    }
}