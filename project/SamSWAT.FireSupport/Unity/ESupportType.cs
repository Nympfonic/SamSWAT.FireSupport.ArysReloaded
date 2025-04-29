using System.Collections.Generic;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public enum ESupportType
{
	None = -1,
	Strafe = 2,
	Extract = 4
}

public class SupportTypeComparer : IEqualityComparer<ESupportType>
{
	public bool Equals(ESupportType x, ESupportType y)
	{
		return x == y;
	}
	
	public int GetHashCode(ESupportType obj)
	{
		return (int)obj;
	}
}