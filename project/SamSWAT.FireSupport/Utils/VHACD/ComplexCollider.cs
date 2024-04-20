using System.Collections.Generic;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.VHACD
{
    [ExecuteInEditMode]
    [AddComponentMenu("Physics/Complex Collider")]
    public class ComplexCollider : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Parameters _parameters;
        public Parameters Parameters => _parameters;

        [SerializeField]
        private ComplexColliderData _colliderData;

        [SerializeField]
        private List<MeshCollider> _colliders = new List<MeshCollider>();

        public List<MeshCollider> Colliders => _colliders;

        [SerializeField, Tooltip("Applies to all child colliders")]
        private bool _isTrigger = false;
        public bool IsTrigger
        {
            set { _isTrigger = value; UpdateColliders(enabled); }
            get { return _isTrigger; }
        }

        [SerializeField, Tooltip("Applies to all child colliders")]
        private PhysicMaterial _material = null;

        public PhysicMaterial Material
        {
            set { _material = value; UpdateColliders(enabled); }
            get { return _material; }
        }

        private void Awake()
        {
            UpdateColliders(true);
        }

        private void UpdateColliders(bool enabled)
        {
            for (int i = 0; i < _colliders.Count; i++)
            {
                if (_colliders[i] == null)
                    continue;

                _colliders[i].isTrigger = _isTrigger;
                _colliders[i].material = _material;
                _colliders[i].convex = true;
                _colliders[i].enabled = enabled;
                if(_colliderData != null && _colliderData.computedMeshes.Length > i)
                {
                    _colliders[i].sharedMesh = _colliderData.computedMeshes[i];
                }
            }
        }

        private void OnDestroy()
        {
            if(_colliders.Count > 0)
            {
                foreach (var item in _colliders)
                {
                    Destroy(item);
                }
                _colliders.Clear();
            }
        }
    }
}
