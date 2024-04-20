using EFT;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles
{
    public interface ITurret
    {
        void AcquireTarget(Player target);
        bool IsAimingAtTarget(Vector3 targetPos);
        bool IsTargetInAimingZone(Vector3 targetPos);
    }
}
