using EFT;
using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks
{
    public class TaskAcquireTarget : Node
    {
        private readonly IController _controller;
        private readonly ITurret _turret;

        public TaskAcquireTarget(IController controller, ITurret turret)
        {
            _controller = controller;
            _turret = turret;
        }

        public override NodeState Evaluate()
        {
            Player target = GetData("target") as Player;

            if (target == null)
            {
                state = NodeState.FAILURE;
                return state;
            }

            Vector3 targetPos = ModHelper.GetBodyPosition(target);

            if (!_turret.IsTargetInAimingZone(targetPos))
            {
                if (!_controller.IsFacingTarget(targetPos))
                {
                    _controller.RotateTowards(targetPos);

                    state = NodeState.RUNNING;
                    return state;
                }

                state = NodeState.FAILURE;
                return state;
            }

            if (_turret.IsAimingAtTarget(targetPos))
            {
                state = NodeState.SUCCESS;
                return state;
            }

            _turret.AcquireTarget(target);

            state = NodeState.RUNNING;
            return state;
        }
    }
}
