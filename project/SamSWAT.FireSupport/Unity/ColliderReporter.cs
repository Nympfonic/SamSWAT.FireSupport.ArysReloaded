﻿using SamSWAT.FireSupport.ArysReloaded.Utils;
using System;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity
{
    public class ColliderReporter : MonoBehaviour, IBatchUpdate
    {
        [NonSerialized] 
        public bool HasCollision;
        private BoxCollider[] _colliders;
        private Collider[] _intersectedColliders;
        private int _mask;        
        
        public void BatchUpdate()
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
            _intersectedColliders = new Collider[5];
            _colliders = GetComponents<BoxCollider>();
            _mask = LayerMask.GetMask("LowPolyCollider", "HighPolyCollider");

            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }
    }
}