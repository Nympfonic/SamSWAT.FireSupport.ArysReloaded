using System.Collections.Generic;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class FireSupportServiceMappings(
	IEqualityComparer<ESupportType> comparer) : Dictionary<ESupportType, IFireSupportService>(comparer)
{
	public bool AnyAvailableRequests()
	{
		if (Count == 0)
		{
			return false;
		}
		
		foreach (IFireSupportService service in Values)
		{
			if (service.IsRequestAvailable())
			{
				return true;
			}
		}
		
		return false;
	}
}