using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public class AH64Task_BeginMoveToPosition : Node
    {
        private AH64Behaviour _ah64;

        public AH64Task_BeginMoveToPosition(AH64Behaviour ah64)
        {
            _ah64 = ah64;
        }

        public override NodeState Evaluate()
        {
            if (_ah64.HasArrivedInActiveField && _ah64.currentMovementState != AH64MovementState.MoveToPosition)
            {
                Vector3 randomPosInActiveField = _ah64.ActiveFieldPosition + Vector3.Scale(Random.insideUnitSphere * _ah64.ActiveFieldRadius, new Vector3(1, 0, 1));
                _ah64.TargetDestination = randomPosInActiveField;
                _ah64.ChangeMovementState(AH64MovementState.MoveToPosition);

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.FAILURE;
            return state;
        }
    }
}
