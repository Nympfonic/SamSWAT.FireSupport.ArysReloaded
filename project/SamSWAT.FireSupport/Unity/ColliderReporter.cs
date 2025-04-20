using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class ColliderReporter : ComponentBase
{
	private bool _hasCollision;
	private BoxCollider[] _colliders;
	private Collider[] _intersectedColliders;
	private int _mask;
	
	public bool HasCollision => _hasCollision;
	
	public override void ManualUpdate()
	{
		foreach (BoxCollider col in _colliders)
		{
			Transform colTransform = col.transform;
			Quaternion colRotation = colTransform.rotation;
			Vector3 center = colTransform.position + colRotation * col.center;
			Vector3 extents = Vector3.Scale(col.size * 0.5f, colTransform.lossyScale);
			
			int hits = Physics.OverlapBoxNonAlloc(
				center,
				extents,
				_intersectedColliders,
				colRotation,
				_mask,
				QueryTriggerInteraction.Ignore);
			
			if (hits > 0)
			{
				_hasCollision = true;
				break;
			}
			
			_hasCollision = false;
		}
	}
	
	private void Start()
	{
		_intersectedColliders = new Collider[5];
		_colliders = GetComponents<BoxCollider>();
		_mask = LayerMask.GetMask("LowPolyCollider", "HighPolyCollider");
	}
	
	private void OnEnable()
	{
		FireSupportPlugin.RegisterComponent(this);
	}
	
	private void OnDisable()
	{
		FireSupportPlugin.DeregisterComponent(this);
	}
}