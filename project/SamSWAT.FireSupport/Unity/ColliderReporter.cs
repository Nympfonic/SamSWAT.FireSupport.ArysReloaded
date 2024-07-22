﻿using System;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class ColliderReporter : MonoBehaviour, IComponent
    {
        [NonSerialized] 
        public bool HasCollision;
        private BoxCollider[] _colliders;
        private Collider[] _intersectedColliders;
        private int _mask;

        public void ManualUpdate()
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
                    _mask,
                    QueryTriggerInteraction.Ignore);

                if (hits > 0)
                {
                    HasCollision = true;
                    break;
                }

                HasCollision = false;
            }
        }

        private void Start()
        {
            FireSupportPlugin.RegisterComponent(this);
            _intersectedColliders = new Collider[5];
            _colliders = GetComponents<BoxCollider>();
            _mask = LayerMask.GetMask("LowPolyCollider", "HighPolyCollider");
        }

        private void OnDestroy()
        {
            FireSupportPlugin.DeregisterComponent(this);
        }
    }
}