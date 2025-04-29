using UnityEngine;
using UnityEngine.UI;

namespace SamSWAT.FireSupport.ArysReloaded.Unity;

public class FireSupportUIElement : ComponentBase
{
	public Image Icon;
	public Image BackgroundImage;
	public Sprite DefaultSubColor;
	public Sprite SelectedSubColor;
	public Text AmountText;
	private bool _isUnderPointer;
	
	public bool IsUnderPointer
	{
		set
		{
			if (_isUnderPointer == value) return;
			_isUnderPointer = value;
			UnderPointerChanged(_isUnderPointer);
		}
	}
	
	protected void UnderPointerChanged(bool isUnderPointer)
	{
		BackgroundImage.sprite = isUnderPointer ? SelectedSubColor : DefaultSubColor;
	}
}