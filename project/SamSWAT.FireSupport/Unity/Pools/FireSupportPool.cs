using System.Collections.Generic;
using UnityEngine;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class FireSupportPool(int size, FireSupportBehaviour prefab, Transform parent)
{
	private readonly Queue<FireSupportBehaviour> _pool = new(size);
	
	protected virtual FireSupportBehaviour Create(FireSupportBehaviour resource)
	{
		FireSupportBehaviour instance = Object.Instantiate(resource, parent);
		instance.gameObject.SetActive(false);
		return instance;
	}
	
	public void Fill()
	{
		for (var i = 0; i < size; i++)
		{
			_pool.Enqueue(Create(prefab));
		}
	}
	
	public FireSupportBehaviour TakeFromPool()
	{
		return _pool.Count == 0 ? Create(prefab) : _pool.Dequeue();
	}
	
	public void ReturnToPool(FireSupportBehaviour obj)
	{
		_pool.Enqueue(obj);
	}
}