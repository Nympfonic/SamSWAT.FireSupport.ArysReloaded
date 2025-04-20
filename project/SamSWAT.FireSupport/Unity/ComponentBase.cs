using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public abstract class ComponentBase : MonoBehaviour
{
	private bool _markedForRemoval;
	
	public abstract void ManualUpdate();
	
	public bool IsMarkedForRemoval()
	{
		return _markedForRemoval;
	}
	
	public void MarkForRemoval()
	{
		_markedForRemoval = true;
	}
}