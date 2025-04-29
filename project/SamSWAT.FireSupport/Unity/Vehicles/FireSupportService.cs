using Cysharp.Threading.Tasks;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public abstract class FireSupportService(int maxRequests) : IFireSupportService
{
	protected int availableRequests = maxRequests;
	protected bool requestAvailable = true;
	
	public abstract ESupportType SupportType { get; }
	public int AvailableRequests => availableRequests;
	
	public bool IsRequestAvailable()
	{
		return availableRequests > 0 && requestAvailable;
	}
	
	public abstract UniTaskVoid PlanRequest();
}