using SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles.AH64
{
    public class AH64Task_CheckArrivedAtLocation : Node
    {
        private AH64Behaviour _ah64;

        public AH64Task_CheckArrivedAtLocation(AH64Behaviour ah64)
        {
            _ah64 = ah64;
        }

        public override NodeState Evaluate()
        {
            if (_ah64.HasArrivedInActiveField)
            {
                _ah64.ChangeMovementState(AH64MovementState.Idle);

                state = NodeState.SUCCESS;
                return state;
            }

            state = NodeState.RUNNING;
            return base.Evaluate();
        }
    }
}
