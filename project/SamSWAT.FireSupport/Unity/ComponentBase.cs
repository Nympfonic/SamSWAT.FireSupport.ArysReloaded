using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public abstract class ComponentBase : MonoBehaviour
{
	protected virtual void Awake()
	{
		useGUILayout = false;
		OnAwake();
	}
	
	protected virtual void Start()
	{
		OnStart();
	}
	
	/// <summary>
	/// Use this method to initialize data within the Unity Awake event function.
	/// </summary>
	protected virtual void OnAwake() {}
	
	/// <summary>
	/// Use this method to initialize data within the Unity Start event function.
	/// </summary>
	protected virtual void OnStart() {}
}