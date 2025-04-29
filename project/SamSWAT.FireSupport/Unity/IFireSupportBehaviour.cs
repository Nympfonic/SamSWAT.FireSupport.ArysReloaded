using System.Threading;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public interface IFireSupportBehaviour
{
	public ESupportType SupportType { get; }
	
	void ProcessRequest(Vector3 position, Vector3 direction, Vector3 rotation, CancellationToken cancellationToken);
	void ReturnToPool();
}