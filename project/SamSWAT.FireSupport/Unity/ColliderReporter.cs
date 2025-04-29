using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class ColliderReporter : UpdatableComponentBase
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
	
	protected override void OnAwake()
	{
		_intersectedColliders = new Collider[5];
		_colliders = GetComponents<BoxCollider>();
		_mask = 1 << LayerMask.NameToLayer("LowPolyCollider") | 1 << LayerMask.NameToLayer("HighPolyCollider");
		
		HasFinishedInitialization = true;
	}
}