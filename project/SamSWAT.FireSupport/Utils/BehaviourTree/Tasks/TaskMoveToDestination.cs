using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks
{
    public class TaskMoveToDestination : Node
    {
        private readonly IController _controller;
        private readonly Vector3 _destination;

        public TaskMoveToDestination(IController controller, Vector3 destination)
        {
            _controller = controller;
            _destination = destination; // Does not work, class is only initialised once, so this parameter won't get updated. Use GetData/SetData instead
        }

        public override NodeState Evaluate()
        {
            // TODO:
            // Fix this shit

            //float sqrMagnitudeToDestination = Vector3.Scale(
            //    _destination - _controller.transform.position,
            //    ModHelper.VECTOR3_IGNORE_Y
            //)
            //.sqrMagnitude;

            //if (sqrMagnitudeToDestination > Mathf.Pow(_closeEnoughRadius, 2))
            //{
            //    state = NodeState.SUCCESS;
            //    return state;
            //}

            if (_controller.HasReachedDestination(_destination))
            {
                state = NodeState.SUCCESS;
                return state;
            }

            _controller.MoveTo(_destination);

            state = NodeState.RUNNING;
            return state;
        }
    }
}
