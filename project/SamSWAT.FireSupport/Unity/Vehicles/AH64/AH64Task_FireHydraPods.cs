using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public class AH64Task_FireHydraPods : Node
    {
        private AH64Behaviour _ah64;

        public AH64Task_FireHydraPods(AH64Behaviour ah64)
        {
            _ah64 = ah64;
        }

        public override NodeState Evaluate()
        {
            Vector3 target = _ah64.ActiveFieldPosition - 120f * Vector3.up;
            if (_ah64.HasArrivedInActiveField)
            {
                state = NodeState.FAILURE;
                return state;
            }
            else if (_ah64.currentMovementState == AH64MovementState.MoveToPosition
                && Vector3.SqrMagnitude(target - _ah64.transform.position) <= Mathf.Pow(200f, 2f)
                && !_ah64.HasFiredHydras && !_ah64.AreHydrasFiring && !_ah64.HasArrivedInActiveField)
            {
                var hydraLeftRaycast = Physics.Raycast(_ah64.hydraLeft.position, _ah64.hydraLeft.forward, out var hydraLeftHit);
                var hydraRightRaycast = Physics.Raycast(_ah64.hydraLeft.position, _ah64.hydraLeft.forward, out var hydraRightHit);
                
                var hydraLeftInRange = Vector3.SqrMagnitude(target - hydraLeftHit.point) <= 625f && hydraLeftHit.distance >= 80f;
                var hydraRightInRange = Vector3.SqrMagnitude(target - hydraRightHit.point) <= 625f && hydraRightHit.distance >= 80f;

                if (hydraLeftRaycast && hydraRightRaycast && hydraLeftInRange && hydraRightInRange)
                {
                    _ah64.FireHydraPods();
                    state = NodeState.SUCCESS;
                    return state;
                }
            }

            state = NodeState.RUNNING;
            return state;
        }
    }
}
