using EFT;
using SamSWAT.FireSupport.ArysReloaded.Utils;
using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree;
using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks;
using System.Collections.Generic;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public class AH64BehaviourTree : Tree
    {
        private const float IN_RANGE_DISTANCE = 5f;
        private const float DETECTION_RADIUS = 300f;
        private const float DETECTION_CLOSE_RADIUS = 15f;
        private const float DETECTION_ANGLE = 160f;
        private const float AIM_DISTANCE_LEEWAY = 2.5f;

        private AH64Behaviour _ah64;

        protected override void Start()
        {
            UpdateManager.Instance.RegisterSlicedUpdate(this, UpdateManager.UpdateMode.Always);

            base.Start();
        }

        protected override Node SetupTree()
        {
            return new Selector(new List<Node>
            {
                // Priority #1
                // Attack with M230 if target visible
                new Sequence(new List<Node>
                {
                    // Either already has a target, or successfully finds another target to proceed
                    new TaskCheckTargets(
                        _ah64.transform,
                        DETECTION_ANGLE,
                        DETECTION_RADIUS,
                        DETECTION_CLOSE_RADIUS,
                        AIM_DISTANCE_LEEWAY
                    ),
                    // Rotate turret/weapon to target
                    new TaskAcquireTarget(_ah64),
                    // Fire weapon
                    new TaskShootWeapon()
                }),
                // Priority #2
                // Move to a new location to try to find new targets
                new TaskMoveToDestination(_ah64, _ah64.TargetDestination, IN_RANGE_DISTANCE)
            });
        }

        private void Awake()
        {
            _ah64 = GetComponent<AH64Behaviour>();
        }

        private void OnDestroy()
        {
            UpdateManager.Instance.DeregisterSlicedUpdate(this);
        }
    }
}
