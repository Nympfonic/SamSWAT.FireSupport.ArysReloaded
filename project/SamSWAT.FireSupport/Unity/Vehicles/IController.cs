using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles
{
    public interface IController
    {
        void MoveTo(Vector3 destination);
        void RotateTowards(Vector3 target);
        bool HasReachedDestination(Vector3 destination);
        bool IsFacingTarget(Vector3 target);
    }
}
