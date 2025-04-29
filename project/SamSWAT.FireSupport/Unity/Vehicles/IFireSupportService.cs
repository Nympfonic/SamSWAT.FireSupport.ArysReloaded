using Cysharp.Threading.Tasks;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public interface IFireSupportService
{
	public ESupportType SupportType { get; }
	public int AvailableRequests { get; }
	
	public bool IsRequestAvailable();
	public UniTaskVoid PlanRequest();
}