using SamSWAT.FireSupport.ArysReloaded.Utils;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity.Vehicles
{
    public abstract class VehicleBehaviour : MonoBehaviour, IBatchUpdate
    {
        public abstract string VehicleName { get; }

        public abstract void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation);
        public abstract void BatchUpdate();

        public void ReturnToPool()
        {
            gameObject.SetActive(false);
        }
    }
}
