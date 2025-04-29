namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public abstract class UpdatableComponentBase : ComponentBase
{
	private bool _markedForRemoval;
	
	public bool HasFinishedInitialization { get; protected set; }
	
	/// <summary>
	/// Executes every frame if the component has finished initialization and is registered into the update loop.
	/// </summary>
	public abstract void ManualUpdate();
	
	/// <summary>
	/// Check if the component has been marked for removal from the update loop.
	/// </summary>
	public bool IsMarkedForRemoval()
	{
		return _markedForRemoval;
	}
	
	/// <summary>
	/// Mark the component for removal from the update loop.
	/// </summary>
	public void MarkForRemoval()
	{
		_markedForRemoval = true;
	}
	
	protected sealed override void Awake()
	{
		base.Awake();
	}
	
	protected sealed override void Start()
	{
		base.Start();
	}
	
	/// <summary>
	/// Unity Event function; Resets "mark for removal" state and registers the component into the update loop.
	/// </summary>
	protected virtual void OnEnable()
	{
		_markedForRemoval = false;
		FireSupportPlugin.Instance.RegisterComponent(this);
	}
	
	/// <summary>
	/// Unity Event function; Marks the component for removal from the update loop.
	/// </summary>
	protected virtual void OnDisable()
	{
		MarkForRemoval();
	}
}