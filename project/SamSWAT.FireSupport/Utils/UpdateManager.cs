using System.Collections.Generic;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils
{
    /// <summary>
    /// Manages all the mod's MonoBehaviour Update and FixedUpdate calls. Only one should exist at a time.
    /// </summary>
    public class UpdateManager : MonoBehaviour
    {
        public enum UpdateMode
        {
            BucketA,
            BucketB,
            Always
        }

        public static UpdateManager Instance { get; private set; }

        private readonly HashSet<IBatchUpdate> _updateBehavioursBucketA = new HashSet<IBatchUpdate>();
        private readonly HashSet<IBatchUpdate> _updateBehavioursBucketB = new HashSet<IBatchUpdate>();
        private bool _isCurrentBucketA;

        private readonly HashSet<IBatchFixedUpdate> _fixedUpdateBehaviours = new HashSet<IBatchFixedUpdate>();

        public void RegisterSlicedUpdate(IBatchUpdate updateBehaviour, UpdateMode updateMode)
        {
            if (updateMode == UpdateMode.Always)
            {
                _updateBehavioursBucketA.Add(updateBehaviour);
                _updateBehavioursBucketB.Add(updateBehaviour);
            }
            else
            {
                HashSet<IBatchUpdate> targetBucket =
                    (updateMode == UpdateMode.BucketA)
                    ? _updateBehavioursBucketA
                    : _updateBehavioursBucketB;

                targetBucket.Add(updateBehaviour);
            }
        }

        public void DeregisterSlicedUpdate(IBatchUpdate updateBehaviour)
        {
            _updateBehavioursBucketA.Remove(updateBehaviour);
            _updateBehavioursBucketB.Remove(updateBehaviour);
        }

        public void RegisterSlicedFixedUpdate(IBatchFixedUpdate updateBehaviour)
        {
            _fixedUpdateBehaviours.Add(updateBehaviour);
        }

        public void DeregisterSlicedFixedUpdate(IBatchFixedUpdate updateBehaviour)
        {
            _fixedUpdateBehaviours.Remove(updateBehaviour);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            HashSet<IBatchUpdate> targetBucket =
                _isCurrentBucketA
                ? _updateBehavioursBucketA
                : _updateBehavioursBucketB;

            foreach (var updateBehaviour in targetBucket)
            {
                updateBehaviour.BatchUpdate();
            }

            _isCurrentBucketA = !_isCurrentBucketA;
        }

        private void FixedUpdate()
        {
            foreach (var updateBehaviour in _fixedUpdateBehaviours)
            {
                updateBehaviour.BatchFixedUpdate();
            }
        }
    }
}
