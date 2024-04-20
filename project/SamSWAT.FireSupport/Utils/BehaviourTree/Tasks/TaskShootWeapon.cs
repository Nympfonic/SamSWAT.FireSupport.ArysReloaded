using SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles;

namespace SamSWAT.FireSupport.ArysReloaded.Utils.BehaviourTree.Tasks
{
    public class TaskShootWeapon : Node
    {
        private readonly IWeapon _weapon;

        public TaskShootWeapon(IWeapon weapon)
        {
            _weapon = weapon;
        }

        public override NodeState Evaluate()
        {
            if (!_weapon.HasAmmo())
            {
                state = NodeState.FAILURE;
                return state;
            }

            _weapon.Shoot();

            state = NodeState.SUCCESS;
            return state;
        }
    }
}
