using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public interface IFireSupportOption
{
	void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation);
	void ReturnToPool();
}